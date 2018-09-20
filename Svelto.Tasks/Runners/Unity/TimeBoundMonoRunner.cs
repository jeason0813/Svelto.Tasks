#if UNITY_5 || UNITY_5_3_OR_NEWER
using System.Diagnostics;
using Svelto.Tasks.Internal.Unity;

namespace Svelto.Tasks.Unity
{
    /// <summary>
    //TimeBoundMonoRunner ensures that the tasks won't take more than maxMilliseconds
    /// </summary>
    public class TimeBoundMonoRunner : MonoRunner
    {
        // Greedy means that the runner will try to occupy the whole maxMilliseconds interval, by looping among all tasks until all are completed or maxMilliseconds passed
        public TimeBoundMonoRunner(string name, float maxMilliseconds, bool mustSurvive = false)
        {
            _flushingOperation = new UnityCoroutineRunner.FlushingOperation();

            UnityCoroutineRunner.InitializeGameObject(name, ref _go, mustSurvive);

            var runnerBehaviour = _go.AddComponent<RunnerBehaviourUpdate>();
            var runnerBehaviourForUnityCoroutine = _go.AddComponent<RunnerBehaviour>();
            UnityCoroutineRunner.RunningTasksInfo info;
            
            info = new TimeBoundRunningInfo(maxMilliseconds) { runnerName = name };

            runnerBehaviour.StartUpdateCoroutine(new UnityCoroutineRunner.Process
                (_newTaskRoutines, _coroutines, _flushingOperation, info,
                 UnityCoroutineRunner.StandardTasksFlushing,
                 runnerBehaviourForUnityCoroutine, StartCoroutine));
        }

        class TimeBoundRunningInfo : UnityCoroutineRunner.RunningTasksInfo
        {
            public TimeBoundRunningInfo(float maxMilliseconds)
            {
                _maxMilliseconds = maxMilliseconds;
            }
            
            public override bool MoveNext(ref int index, int count)
            {
                if (index == 0)
                {
                    _stopWatch.Reset();
                    _stopWatch.Start();
                }

                if (_stopWatch.ElapsedMilliseconds > _maxMilliseconds)
                    return false;
                 
                return true;
            }
            
            readonly Stopwatch _stopWatch = new Stopwatch();
            readonly float _maxMilliseconds;

        }
    }
}
#endif