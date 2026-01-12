using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solocaster.Entities;
using System;

namespace MonoRaycaster;

public class Camera
{
    private Vector2 _position = new(18, 3); 
    private Vector2 _direction = new(-1, 0);
    private Vector2 _plane = new(0, .45f);

    private readonly Map _map;

    public Camera(Map map)
    {
        _map = map;
    }

    public void Update(GameTime gameTime)
    {
        float ms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        float moveSpeed = ms * .015f;
        float rotSpeed = ms * .005f;

        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.E) && TryOpenDoor())
            return;

        float moveAmount = 0;
        if (keyboardState.IsKeyDown(Keys.W))
            moveAmount = moveSpeed;
        else if (keyboardState.IsKeyDown(Keys.S))
            moveAmount = -moveSpeed;

        if (moveAmount != 0)
        {
            var moveStep = _direction * moveAmount;
            if (!_map.IsBlocked((int)(_position.X + moveStep.X), (int)_position.Y))
                _position.X += moveStep.X;

            if (!_map.IsBlocked((int)_position.X, (int)(_position.Y + moveStep.Y)))
                _position.Y += moveStep.Y;
        }

        if (keyboardState.IsKeyDown(Keys.A))
        {
            Vector2 oldDirection = _direction;
            var cos = MathF.Cos(-rotSpeed);
            var sin = MathF.Sin(-rotSpeed);

            _direction.X = _direction.X * cos - _direction.Y * sin;
            _direction.Y = oldDirection.X * sin + _direction.Y * cos;

            Vector2 oldPlane = _plane;
            _plane.X = _plane.X * cos - _plane.Y * sin;
            _plane.Y = oldPlane.X * sin + _plane.Y * cos;
        }
        else if (keyboardState.IsKeyDown(Keys.D))
        {
            Vector2 oldDirection = _direction;
            var cos = MathF.Cos(rotSpeed);
            var sin = MathF.Sin(rotSpeed);

            _direction.X = _direction.X * cos - _direction.Y * sin;
            _direction.Y = oldDirection.X * sin + _direction.Y * cos;

            Vector2 oldPlane = _plane;
            _plane.X = _plane.X * cos - _plane.Y * sin;
            _plane.Y = oldPlane.X * sin + _plane.Y * cos;
        }
    }

    private bool TryOpenDoor()
    {
        float checkDistance = 1.5f;

        for (float dist = 0.1f; dist <= checkDistance; dist += 0.1f)
        {
            int checkX = (int)(_position.X + _direction.X * dist);
            int checkY = (int)(_position.Y + _direction.Y * dist);

            var door = _map.GetDoor(checkX, checkY);
            if (door is not null)
            {   
                door.StartOpening();
                return true;
            }
        }

        return false;
    }


    public Vector2 Position => _position;
    public Vector2 Direction => _direction;
    public Vector2 Plane => _plane;
}
