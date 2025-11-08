using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace RealProject
{
    public class BoxCollider : Collider
    {
        public Vector2 colliderSize;

        public BoxCollider(Vector2 offset, bool trig, Vector2 size) : base(offset, trig)
        {
            colliderSize = size;
        }

        public override bool CheckPlayerCollision(Vector2 tileCentre, Vector2 playerPos, Vector2 playerColliderSize, Vector2 playerColliderOffset)
        {
            bool colliding = false;

            bool verticalsInbetween = false;

            float top = playerPos.Y + playerColliderOffset.Y - playerColliderSize.Y / 2;
            float left = playerPos.X + playerColliderOffset.X - playerColliderSize.X / 2;
            float right = playerPos.X + playerColliderOffset.X + playerColliderSize.X / 2;
            float bottom = playerPos.Y + playerColliderOffset.Y + playerColliderSize.Y / 2;

            Vector2 colliderCentre = tileCentre + colliderOffset;

            if (top > colliderCentre.Y - colliderSize.Y / 2 && top < colliderCentre.Y + colliderSize.Y / 2)
            {
                verticalsInbetween = true;
            }
            else if (bottom > colliderCentre.Y - colliderSize.Y / 2 && bottom < colliderCentre.Y + colliderSize.Y / 2)
            {
                verticalsInbetween = true;
            }

            if (verticalsInbetween)
            {
                if (left > colliderCentre.X - colliderSize.X / 2 && left < colliderCentre.X + colliderSize.X / 2)
                {
                    //Debug.WriteLine("Intersecting");
                    colliding = true;
                }
                else if (right > colliderCentre.X - colliderSize.X / 2 && right < colliderCentre.X + colliderSize.X / 2)
                {
                    //Debug.WriteLine("Intersecting");
                    colliding = true;
                }
            }

            return colliding;
        }

        public override Vector2 ResolveCollision(Vector2 tileCentre, Vector2 playerPos, Vector2 playerColliderSize, Vector2 playerColliderOffset)
        {
            Vector2 resolvedPosition = playerPos;
            Vector2 colliderPos = tileCentre + colliderOffset;

            Vector2 playerColliderPos = playerPos + playerColliderOffset;

            List<Vector2> playerCorners = new List<Vector2>();

            playerCorners.Add(playerColliderPos + -playerColliderSize / 2);
            playerCorners.Add(playerColliderPos + new Vector2(playerColliderSize.X, -playerColliderSize.Y) / 2);
            playerCorners.Add(playerColliderPos + new Vector2(-playerColliderSize.X, playerColliderSize.Y) / 2);
            playerCorners.Add(playerColliderPos + playerColliderSize / 2);

            Vector2 dir = playerCorners[0] - colliderPos;

            foreach (var pc in playerCorners)
            {
                Vector2 dir1 = pc - colliderPos;

                if (dir1.Length() < dir.Length())
                    dir = dir1;
            }

            if (MathF.Abs(dir.X) > MathF.Abs(dir.Y))
            {
                if (dir.X < 0)
                {
                    resolvedPosition.X = colliderPos.X - (colliderSize.X / 2) - (playerColliderSize.X / 2) - playerColliderOffset.X;
                }
                else
                {
                    resolvedPosition.X = colliderPos.X + (colliderSize.X / 2) + (playerColliderSize.X / 2) - playerColliderOffset.X;
                }
            }
            if (MathF.Abs(dir.X) < MathF.Abs(dir.Y))
            {
                if (dir.Y < 0)
                {
                    resolvedPosition.Y = colliderPos.Y - (colliderSize.Y / 2) - (playerColliderSize.Y / 2) - playerColliderOffset.Y;
                }
                else
                {
                    resolvedPosition.Y = colliderPos.Y + (colliderSize.Y / 2) + (playerColliderSize.Y / 2) - playerColliderOffset.Y;
                }
            }

            return resolvedPosition;
        }
    }
}
