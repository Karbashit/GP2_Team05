using System.Collections.Generic;
using Andreas.Scripts;
using Andreas.Scripts.HealingZone;
using AudioSystem;
using Health;
using UnityEngine;
using DG.Tweening;
using Util;

namespace Joakim.Scripts.Mechanics {
    public class PowerCoil : MonoBehaviour {
        private HealthSystem.IDamagable affectedPlayer;

        public int requiredTicsToExecuteEvent;
        public float tickRate = 1;
        private int _currentTic = 0;
        public List<GameObject> objectToDisableUponExecute = new List<GameObject>();
        private GameObject _lightningTrailEffect;
        private Transform _affectedPlayerPos;
        public float damagePerTic;
        public HealingZone coupledHealingStation;
        public GameObject coupledFuseBox;
        private LineRenderer _lr;
        private int _currentLineIndex;
        public bool done;

        [Space(8)] [Header("SFX")] [SerializeField]
        private AudioClip _sfxIdle;

        [SerializeField] private AudioClip _sfxCharging;
        [SerializeField] private AudioClip _sfxOnComplete;
        private AudioSource _sfxIdleSource, _sfxChargingSource, _sfxOnCompleteSource;

        [Header(
            "Check this only if more than one coil needs to be charged to progress. Do the same for other coils connected to this event.")]
        public bool multipleCoils;

        private Vector3 _lineStartPos, _lineEndPos;

        [Header("Only check this for the final powercoil interaction of the level.")]
        public bool endPowerCoil;

        private void Awake() {
            _lightningTrailEffect = FastResources.Load<GameObject>("TeslaLightningTrail");
            var fuseBoxPos = coupledFuseBox.transform.position;
            _lineStartPos = transform.position;
            _lineEndPos = fuseBoxPos;
        }

        private void Start() {
            _lr = GetComponentInChildren<LineRenderer>();
            _lr.transform.position = Vector3.zero;
            _lr.SetPosition(0, _lineStartPos); //endpos
            _lr.SetPosition(1, _lineStartPos); //origin
        }

        private void OnTriggerEnter(Collider other) {
            var player = other.gameObject.GetComponent<Player>();
            if (player == null)
                return;

            if (!coupledHealingStation.IsOccupied) {
                player.SfxData.GoToHealPad.Play(player.transform.position);
            }

            if (affectedPlayer == null && !done) {
                affectedPlayer = player;
                _affectedPlayerPos = other.transform;
                InvokeRepeating(nameof(OfferHealth), 0, tickRate);
                Debug.Log("STARTED COIL");

                if (_sfxCharging != null)
                    _sfxChargingSource = AudioManager.PlaySfx(_sfxCharging.name, transform.position);
            }
        }

        private void OnTriggerExit(Collider other) {
            var player = other.gameObject.GetComponent<Player>();
            if (player == null)
                return;

            if (other.GetComponent<HealthSystem.IDamagable>() == affectedPlayer) {
                if (!done) {
                    _lr.SetPosition(0, _lineStartPos);
                }

                _sfxChargingSource?.Stop();

                _currentLineIndex = 0;
                _currentTic = 0;
                affectedPlayer = null;
                CancelInvoke(nameof(OfferHealth));
            }
        }

        private void OfferHealth() {
            if (_currentTic != requiredTicsToExecuteEvent && coupledHealingStation.IsOccupied) {
                _currentLineIndex++;
                var distance = Vector3.Distance(_lineStartPos, _lineEndPos);
                var dir = (_lineEndPos - _lineStartPos).normalized;
                var step = distance / requiredTicsToExecuteEvent;
                var totalSteps = step * _currentLineIndex;
                var position = _lineStartPos + dir * totalSteps;
                _lr.SetPosition(0, position);
                var lightningEffect = Instantiate(_lightningTrailEffect, transform.position, Quaternion.identity);
                lightningEffect.transform.DOMove(_affectedPlayerPos.position, 0.25f)
                    .OnComplete(() => Destroy(lightningEffect));
                affectedPlayer?.Health.InstantDamage(affectedPlayer, damagePerTic);
                _currentTic++;
                Debug.Log($"TICKED COIL ({_currentTic} / {requiredTicsToExecuteEvent})");
            }

            else if (_currentTic == requiredTicsToExecuteEvent) {
                done = !done;
                ExecuteEvent();
                CancelInvoke(nameof(OfferHealth));
            }
        }

        public void ExecuteEvent() {
            Debug.Log("COMPLETED COIL");
            var pos = transform.position;
            if (_sfxIdle != null) {
                _sfxIdleSource = AudioManager.PlaySfx(_sfxIdle.name, pos);
                _sfxIdleSource.SetMaxDistance(20f);
            }

            if (_sfxOnComplete != null) {
                _sfxOnCompleteSource = AudioManager.PlaySfx(_sfxOnComplete.name, pos);
            }

            if (!multipleCoils && !endPowerCoil) {
                foreach (var door in objectToDisableUponExecute) {
                    door.GetComponent<DoorOpen>().DoorSlideOpen();
                }
            }
            else if (multipleCoils) {
                coupledFuseBox.GetComponent<FuseBoxMultipleCoilsOnly>().CheckIfFinished();
            }

            else if (endPowerCoil) {
                GameManager.Instance.Ending();
            }
        }
    }
}