using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace RealProject
{
    public abstract class Collider
    {
        public Vector2 colliderOffset;
        public bool isTrigger;

        public bool colliding;

        public event TriggerEventHandler TriggerEnter;

        public Collider(Vector2 offset, bool trig)
        {
            colliderOffset = offset;
            isTrigger = trig;
        }

        public abstract bool CheckPlayerCollision(Vector2 a, Vector2 b, Vector2 c, Vector2 d);
        public abstract Vector2 ResolveCollision(Vector2 a, Vector2 b, Vector2 c, Vector2 d);
        public delegate void TriggerEventHandler(Collider collider);

        public virtual void OnTriggerEnter()
        {
            TriggerEnter?.Invoke(this);
        }
    }
}
