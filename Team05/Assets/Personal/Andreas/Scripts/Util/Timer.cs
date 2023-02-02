﻿using System;
using UnityEngine;

namespace Util
{
    [Serializable]
    public struct Timer
    {
        public static implicit operator Timer(float time) => new Timer(time);
        
        [SerializeField] private float _time;
        private float _timer;
        
        public Timer(float tickRate)
        {
            _time = tickRate;
            _timer = 0f;
        }

        public bool UpdateTick()
        {
            _timer += Time.deltaTime;
            if(_timer >= _time)
            {
                _timer -= _time;
                return true;
            }

            return false;
        }
        
    }
}