// ============================================================
// VfxPool.cs — small pool for runtime uGUI ParticleSystems
// ============================================================

using System.Collections.Generic;
using UnityEngine;

namespace Xiuxian.Presentation.Vfx
{
    public sealed class VfxPool
    {
        private readonly RectTransform parent;
        private readonly List<VfxInstance> instances = new();

        public VfxPool(RectTransform parent, int prewarmCount)
        {
            this.parent = parent;
            for (var i = 0; i < prewarmCount; i++) instances.Add(CreateInstance());
        }

        public VfxInstance Acquire()
        {
            for (var i = 0; i < instances.Count; i++)
            {
                if (!instances[i].InUse) return instances[i];
            }
            var instance = CreateInstance();
            instances.Add(instance);
            return instance;
        }

        public void StopAll()
        {
            for (var i = 0; i < instances.Count; i++) instances[i].Release();
        }

        private VfxInstance CreateInstance()
        {
            var go = new GameObject("VfxParticle", typeof(RectTransform), typeof(ParticleSystem), typeof(UiParticleGraphic));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero;
            var particleSystem = go.GetComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            if (renderer != null) renderer.enabled = false;
            var main = particleSystem.main;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = Presentation.Animation.PresentationTunings.VfxMaxParticles;
            go.SetActive(false);
            return new VfxInstance(go, rect, particleSystem);
        }
    }

    public sealed class VfxInstance
    {
        public VfxInstance(GameObject gameObject, RectTransform rectTransform, ParticleSystem particleSystem)
        {
            GameObject = gameObject;
            RectTransform = rectTransform;
            ParticleSystem = particleSystem;
        }

        public GameObject GameObject { get; }
        public RectTransform RectTransform { get; }
        public ParticleSystem ParticleSystem { get; }
        public bool InUse { get; private set; }

        public void Play(Vector2 position, VfxEffectDescriptor descriptor)
        {
            if (descriptor == null) return;
            InUse = true;
            GameObject.name = "VfxParticle_" + descriptor.Name;
            GameObject.SetActive(true);
            RectTransform.anchoredPosition = position + descriptor.LocalOffset;
            Configure(ParticleSystem, descriptor);
            ParticleSystem.Clear(true);
            ParticleSystem.Play(true);
        }

        public void Release()
        {
            if (!InUse && !GameObject.activeSelf) return;
            ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            InUse = false;
            GameObject.SetActive(false);
        }

        private static void Configure(ParticleSystem particleSystem, VfxEffectDescriptor descriptor)
        {
            var main = particleSystem.main;
            main.duration = Mathf.Max(0.05f, descriptor.Duration);
            main.loop = descriptor.Looping;
            main.startLifetime = new ParticleSystem.MinMaxCurve(descriptor.LifetimeMin, descriptor.LifetimeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(descriptor.StartSpeedMin, descriptor.StartSpeedMax);
            main.startSize = new ParticleSystem.MinMaxCurve(descriptor.StartSizeMin, descriptor.StartSizeMax);
            main.startColor = descriptor.StartColor;
            main.maxParticles = Presentation.Animation.PresentationTunings.VfxMaxParticles;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = particleSystem.emission;
            emission.enabled = true;
            emission.rateOverTime = descriptor.EmissionRate;
            emission.SetBursts(descriptor.BurstCount > 0
                ? new[] { new ParticleSystem.Burst(0f, (short)descriptor.BurstCount) }
                : System.Array.Empty<ParticleSystem.Burst>());

            var shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = descriptor.Shape == VfxShape.Cone ? ParticleSystemShapeType.Cone : ParticleSystemShapeType.Circle;
            shape.radius = descriptor.Radius;
            shape.radiusThickness = 0.75f;
            shape.angle = descriptor.ConeAngle;

            var color = particleSystem.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(descriptor.StartColor, 0f),
                    new GradientColorKey(descriptor.EndColor, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(descriptor.StartColor.a, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            color.color = gradient;
        }
    }
}
