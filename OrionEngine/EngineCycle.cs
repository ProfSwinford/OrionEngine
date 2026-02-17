using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrionEngine
{
    internal enum EngineCycle
    {
        //Comment out unnecessary options to limit our options for the time being. We'll add more as we need them.

        Awake,
        OnEnable,
        //Reset,
        Start,
        FixedUpdate,
        //InternalPhysicsUpdate,
        //InternalAnimationUpdate,
        //OnTriggerEnter,
        //OnTriggerStay,
        //OnTriggerExit,
        //OnCollisionEnter,
        //OnCollisionStay,
        //OnCollisionExit,
        //YieldWaitForFixedUpdate,
        //AwaitableFixedUpdateAsync,
        OnInputEvents,
        Update,
        //YieldNull,
        //YieldWaitForSeconds,
        //YieldWWW,
        //YieldStartCoroutine,
        //AsyncTaskContinuation,
        //AwaitableNextFrameAsync,
        //LateInternalAnimationUpdate,
        LateUpdate,
        //OnPreCull,
        //OnBecameInvisible,
        //OnWillRenderObject,
        //OnPreRender,
        //OnRenderObject,
        //OnPostRender,
        OnRenderImage,
        //OnDrawGizmos,
        //YieldWaitForEndOfFrame,
        //AwaitableEndOfFrameAsync,
        //OnApplicationPause,
        OnApplicationQuit,
        OnDisable,
        OnDestroy
    }
}
