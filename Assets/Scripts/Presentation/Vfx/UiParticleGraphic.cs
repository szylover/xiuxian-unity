// ============================================================
// UiParticleGraphic.cs — draws ParticleSystem particles as uGUI quads
// ============================================================

using UnityEngine;
using UnityEngine.UI;

namespace Xiuxian.Presentation.Vfx
{
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class UiParticleGraphic : MaskableGraphic
    {
        private static Texture2D particleTexture;
        private ParticleSystem particleSystem;
        private ParticleSystem.Particle[] particles;

        public override Texture mainTexture => ParticleTexture;

        private static Texture2D ParticleTexture
        {
            get
            {
                if (particleTexture != null) return particleTexture;
                particleTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    name = "RuntimeVfxParticleTexture",
                    hideFlags = HideFlags.HideAndDontSave
                };
                particleTexture.SetPixel(0, 0, Color.white);
                particleTexture.Apply(false, true);
                return particleTexture;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            particleSystem = GetComponent<ParticleSystem>();
            particles = new ParticleSystem.Particle[Presentation.Animation.PresentationTunings.VfxMaxParticles];
            raycastTarget = false;
        }

        private void LateUpdate()
        {
            if (particleSystem != null && particleSystem.IsAlive(true)) SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if (particleSystem == null) return;
            var count = particleSystem.GetParticles(particles);
            for (var i = 0; i < count; i++)
            {
                var particle = particles[i];
                var size = particle.GetCurrentSize(particleSystem) * Presentation.Animation.PresentationTunings.VfxUiParticleScale;
                var half = size * 0.5f;
                var center = (Vector2)particle.position;
                var color32 = (Color32)particle.GetCurrentColor(particleSystem);
                var index = vh.currentVertCount;
                vh.AddVert(center + new Vector2(-half, -half), color32, new Vector2(0f, 0f));
                vh.AddVert(center + new Vector2(-half, half), color32, new Vector2(0f, 1f));
                vh.AddVert(center + new Vector2(half, half), color32, new Vector2(1f, 1f));
                vh.AddVert(center + new Vector2(half, -half), color32, new Vector2(1f, 0f));
                vh.AddTriangle(index, index + 1, index + 2);
                vh.AddTriangle(index + 2, index + 3, index);
            }
        }
    }
}
