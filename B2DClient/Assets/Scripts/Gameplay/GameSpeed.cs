using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSpeed
{
    

    public struct MoveSpeed {
        public float speed;

        public MoveSpeed(float s)
        {
            speed = s * Constants.GameSpeed;
        }
        public static MoveSpeed operator +(MoveSpeed a, MoveSpeed b) {
            MoveSpeed ms = new MoveSpeed(0);
            ms.speed = a.speed + b.speed;
            return ms;
        }
        //public static MoveSpeed operator +(MoveSpeed s, float a) => new MoveSpeed(a);
    }

    public struct TimeInterval
    {
        public float interval { private set; get; }
        public TimeInterval(float ti) { interval = ti / Constants.GameSpeed; }
        public static TimeInterval operator *(TimeInterval ti, float f) => new TimeInterval(ti.interval * f);
    }

}
