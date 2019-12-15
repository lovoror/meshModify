/******************************************************************************/
/*
  Project   - Boing Kit
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace BoingKit
{
  internal static class BoingWorkSynchronous
  {
    #region Behavior
    
    internal static void UpdateBehaviorsLateUpdate(Dictionary<int, BoingBehavior> behaviorMap)
    {
      float dt = Time.deltaTime;
      foreach (var itBehavior in behaviorMap)
      {
        var behavior = itBehavior.Value;
        behavior.PrepareExecute();
        behavior.Execute(dt);
        behavior.PullResults();
      }
    }

    #endregion // Behavior


    #region Reactor

    internal static void UpdateReactorsLateUpdate
    (
      BoingEffector.Params[] aEffectorParams, 
      Dictionary<int, BoingReactor> reactorMap, 
      Dictionary<int, BoingReactorField> fieldMap, 
      Dictionary<int, BoingReactorFieldCPUSampler> cpuSamplerMap
    )
    {
      float dt = Time.deltaTime;

      Profiler.BeginSample("Update Reactors");

      foreach (var itReactor in reactorMap)
      {
        var reactor = itReactor.Value;

        reactor.PrepareExecute();

        for (int i = 0; i < aEffectorParams.Length; ++i)
          reactor.Params.AccumulateTarget(ref aEffectorParams[i]);
        reactor.Params.EndAccumulateTargets();

        reactor.Execute(dt);
        reactor.PullResults();
      }
      Profiler.EndSample();

      Profiler.BeginSample("Update Fields (CPU)");
      foreach (var itField in fieldMap)
      {
        var field = itField.Value;
        switch (field.HardwareMode)
        {
          case BoingReactorField.HardwareModeEnum.CPU:
            field.ExecuteCpu(dt);
            break;
        }
      }
      Profiler.EndSample();

      Profiler.BeginSample("Update Field Samplers");
      foreach (var itSampler in cpuSamplerMap)
      {
        var sampler = itSampler.Value;
        sampler.SampleFromField();
      }
      Profiler.EndSample();
    }

    #endregion // Reactor


    #region Bones

    // use fixed time step for bones because they involve collision resolution
    internal static void UpdateBonesLateUpdateExecute
    (
      BoingEffector.Params[] aEffectorParams, 
      Dictionary<int, BoingBones> bonesMap
    )
    {
      Profiler.BeginSample("Update Bones (Execute)");

      foreach (var itBones in bonesMap)
      {
        var bones = itBones.Value;
        bones.PrepareExecute();

        for (int i = 0; i < aEffectorParams.Length; ++i)
          bones.AccumulateTarget(ref aEffectorParams[i]);
        bones.EndAccumulateTargets();

        for (int iteration = 0; iteration < BoingManager.NumFixedUpdateIterations; ++iteration)
          bones.Params.Execute(bones, BoingManager.FixedDeltaTime);
      }

      Profiler.EndSample();
    }

    internal static void UpdateBonesLateUpdatePullResults
    (
      BoingEffector.Params[] aEffectorParams,
      Dictionary<int, BoingBones> bonesMap
    )
    {
      Profiler.BeginSample("Update Bones (Pull Results)");

      foreach (var itBones in bonesMap)
      {
        var bones = itBones.Value;

        bones.Params.PullResults(bones);
      }

      Profiler.EndSample();
    }

    #endregion // Bones
  }
}
