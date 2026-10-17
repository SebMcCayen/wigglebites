using UnityEngine;
using Wigglebites.Application;

namespace Wigglebites.Presentation
{
    public sealed class ArenaFeedback : MonoBehaviour
    {
        private ArenaBootstrap _game;
        private ArenaView _view;
        private AudioSource _audioSource;
        private ParticleSystem _particles;
        private AudioClip _pickupClip;
        private AudioClip _eliminationClip;
        private AudioClip _resultClip;
        private MatchController _observedMatch;
        private MatchPhase _lastPhase;
        private float _lastMass;
        private int _lastEliminations;

        public void Configure(ArenaBootstrap game, ArenaView view)
        {
            _game = game;
            _view = view;
        }

        private void Awake()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.volume = 0.32f;

            var particleObject = new GameObject("Feedback Particles");
            particleObject.transform.SetParent(transform, false);
            _particles = particleObject.AddComponent<ParticleSystem>();
            var main = _particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.55f;
            main.startSpeed = 3.5f;
            main.startSize = 0.28f;
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = _particles.emission;
            emission.enabled = false;
            var shape = _particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.4f;
            _particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            _pickupClip = CreateTone("Pickup", 660f, 0.09f);
            _eliminationClip = CreateTone("Elimination", 330f, 0.16f);
            _resultClip = CreateTone("Result", 523.25f, 0.28f);
        }

        private void Update()
        {
            if (_game == null || _game.Match == null || _game.Player == null) return;
            if (_observedMatch != _game.Match)
            {
                _observedMatch = _game.Match;
                _lastPhase = _observedMatch.Phase;
                _lastMass = _game.Player.Mass;
                _lastEliminations = _game.Player.Eliminations;
                return;
            }

            var player = _game.Player;
            if (player.Mass > _lastMass + 0.001f)
            {
                Emit(new Color(1f, 0.83f, 0.24f), 7, _pickupClip);
            }
            if (player.Eliminations > _lastEliminations)
            {
                Emit(new Color(0.2f, 0.9f, 1f), 18, _eliminationClip);
            }
            if (_lastPhase != MatchPhase.Results && _observedMatch.Phase == MatchPhase.Results)
            {
                Emit(new Color(1f, 0.78f, 0.18f), _observedMatch.PlayerRank == 1 ? 36 : 18, _resultClip);
            }

            _lastMass = player.Mass;
            _lastEliminations = player.Eliminations;
            _lastPhase = _observedMatch.Phase;
        }

        private void Emit(Color color, int count, AudioClip clip)
        {
            if (!_game.ReducedEffects && _view != null)
            {
                _particles.transform.position = _view.GetSnakeHeadPosition(1) + (Vector3.up * 0.5f);
                var parameters = new ParticleSystem.EmitParams
                {
                    startColor = color,
                    startSize = 0.22f
                };
                _particles.Emit(parameters, count);
            }
            if (_game.SoundEnabled && clip != null) _audioSource.PlayOneShot(clip);
        }

        private static AudioClip CreateTone(string clipName, float frequency, float duration)
        {
            const int sampleRate = 22050;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[sampleCount];
            for (var index = 0; index < sampleCount; index++)
            {
                var time = index / (float)sampleRate;
                var envelope = 1f - (index / (float)sampleCount);
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * time) * envelope * 0.35f;
            }
            var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
