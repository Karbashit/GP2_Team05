using System;
using Andreas.Scripts;
using UnityEngine;

namespace Health {
    public class HealthSystem {
        public float Health;

        public const float MinHp = -0.5f;
        public const float MaxHp = 0.5f;

        public event Action OnDamageTaken;
        public event Action OnDie;

        public HealthSystem() {
            Health = 0.25f;
            Health = Mathf.Clamp(Health, -0.5f, 0.5f);
        }
        
        public void TransferHealth(IDamagable self, IDamagable other) {
            if (self.Health.Health >= -0.40f && other.Health.Health < 0.5f) {
                InstantHealing(other, 0.05f);

                InstantDamage(self, 0.05f, false);

                var otherHealthMatFloat = other.HealthMaterial.GetFloat("_HP");
            }
        }

        public void InstantHealing(HealthSystem.IDamagable player, float value) {
            player.Health.Health = ClampHP(player.Health.Health + value);
            var playerHealthMat = player.HealthMaterial.GetFloat("_HP");
            player.HealthMaterial.SetFloat("_HP", ClampHP(playerHealthMat + value));
        }

        public void InstantDamage(HealthSystem.IDamagable player, float value, bool invokeEvent = true) {
            if (player.Health.Health >= -0.45f) {
                player.Health.Health = ClampHP(player.Health.Health - value);
                var playerHealthMat = player.HealthMaterial.GetFloat("_HP");
                player.HealthMaterial.SetFloat("_HP",ClampHP(playerHealthMat - value));
                
                playerHealthMat = player.HealthMaterial.GetFloat("_HP");
                if(invokeEvent)
                    OnDamageTaken?.Invoke();
            }
            else {
                Die(player);
            }
        }

        public void ResetHealth(IDamagable player) {
            player.Health.Health = 0.5f;
            player.HealthMaterial.SetFloat("_HP", 0.5f);
        }

        public void Die(IDamagable damagable) {
            OnDie?.Invoke();
            GameManager.Instance.PlayersDead();
        }

        private float ClampHP(float health) => Mathf.Clamp(health, -0.5f, 0.5f);


        public interface IDamagable {
            public Material HealthMaterial { get; set; }
            public HealthSystem Health { get; set; }
            public float Energy { get; set; }
        }
    }

    public class Healing {
        public void Hot(HealthSystem.IDamagable player, float value, float tickRate, float duration) {
        }
    }

    public class Damage {
        public void Dot(HealthSystem.IDamagable player, float value, float tickRate, float duration) {
        }
    }
}