/*
 tdogl.Camera
 
 Copyright 2012 Thomas Dalling - http://tomdalling.com/
 C# Port is made by Vyacheslav Zeronov - zeronsix@gmail.com
 C# Port is based on Pencil.Gaming library by Antonie Blom - https://github.com/antonijn/Pencil.Gaming

 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
 */

using System;
using Pencil.Gaming.MathUtils;

namespace tdogl
{
    /// <summary>
    /// A first-person shooter type of camera.
    ///
    /// Set the properties of the camera, then use the `matrix` method to get the camera matrix for
    /// use in the vertex shader.
    ///
    /// Includes the perspective projection matrix.
    /// </summary>
    public class Camera
    {
        private const float MaxVerticalAngle = 85.0f; // must be less than 90 to avoid gimbal lock

        public Camera()
        {
            _position = new Vector3(0.0f, 0.0f, 1.0f);
            _horizontalAngle = 0.0f;
            _verticalAngle = 0.0f;
            _fieldOfView = MathHelper.ToRadians(50.0f);
            _nearPlane = 0.01f;
            _farPlane = 100.0f;
            _viewportAspectRatio = 4.0f / 3.0f;
        }

        /// <summary>
        /// The position of the camera.
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public void OffsetPosition(Vector3 offset)
        {
            _position += offset;
        }

        /// <summary>
        /// The vertical viewing angle of the camera, in degrees.
        ///
        /// Determines how "wide" the view of the camera is. Large angles appear to be zoomed out,
        /// as the camera has a wide view. Small values appear to be zoomed in, as the camera has a
        /// very narrow view.
        ///
        //// The value must be between 0 and 180.
        /// </summary>
        public float FieldOfView
        {
            get { return MathHelper.ToDegrees(_fieldOfView); }
            set { _fieldOfView = MathHelper.ToRadians(value); }
        }

        /// <summary>
        /// The closest visible distance from the camera.
        ///
        /// Objects that are closer to the camera than the near plane distance will not be visible.
        ///
        /// Value must be greater than 0.
        /// </summary>
        public float NearPlane
        {
            get { return _nearPlane; }
        }

        /// <summary>
        /// The farthest visible distance from the camera.
        ///
        /// Objects that are further away from the than the far plane distance will not be visible.
        ///
        /// Value must be greater than the near plane
        /// </summary>
        public float FarPlane
        {
            get { return _farPlane; }
        }

        /// <summary>
        /// Sets the near and far plane distances.
        ///
        /// Everything between the near plane and the var plane will be visible. Everything closer
        /// than the near plane, or farther than the far plane, will not be visible.
        /// </summary>
        /// <param name="nearPlane">Minimum visible distance from camera. Must be > 0</param>
        /// <param name="farPlane">Maximum visible distance from vamera. Must be > nearPlane</param>
        public void SetNearAndFarPlanes(float nearPlane, float farPlane)
        {
            if (nearPlane < 0.0f) throw new ArgumentOutOfRangeException("nearPlane");
            if (nearPlane < farPlane) throw new ArgumentException("nearPlane must be less that farPlane!");
            _nearPlane = nearPlane;
            _farPlane = farPlane;
        }

        /// <summary>
        /// A rotation matrix that determines the direction the camera is looking.
        ///
        /// Does not include translation (the camera's position).
        /// </summary>
        public Matrix Orientation
        {
            get
            {
                Matrix orientation;
                orientation = Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(_horizontalAngle));
                orientation *= Matrix.CreateFromAxisAngle(new Vector3(1, 0, 0), MathHelper.ToRadians(_verticalAngle));
                return orientation;
            }
        }

        /// <summary>
        /// Offsets the cameras orientation.
        ///
        /// The verticle angle is constrained between 85deg and -85deg to avoid gimbal lock.
        /// </summary>
        /// <param name="upAngle">the angle (in degrees) to offset upwards. Negative values are downwards.</param>
        /// <param name="rightAngle">the angle (in degrees) to offset rightwards. Negative values are leftwards.</param>
        public void OffsetOrientation(float upAngle, float rightAngle)
        {
            _horizontalAngle += rightAngle;
            _verticalAngle += upAngle;
            NormalizeAngles();
        }

        /// <summary>
        /// Orients the camera so that is it directly facing `position`
        /// </summary>
        /// <param name="position">the position to look at</param>
        public void LookAt(Vector3 position)
        {
            if (position == _position) throw new ArgumentException("position is equal to _position!");
            Vector3 direction = position - _position;
            direction.Normalize();
            _verticalAngle = (float)MathHelper.ToDegrees(Math.Asin(-direction.Y));
            _horizontalAngle = (float)MathHelper.ToDegrees(Math.Atan2(-direction.X, -direction.Z));
            NormalizeAngles();
        }

        /// <summary>
        /// The width divided by the height of the screen/window/viewport
        ///
        /// Incorrect values will make the 3D scene look stretched.
        /// </summary>
        public float ViewportAspectRatio
        {
            get { return _viewportAspectRatio; }
            set
            {
                if (value <= 0.0f) throw new ArgumentOutOfRangeException("ViewportAspectRatio");
                _viewportAspectRatio = value;
            }
        }

        /// <summary>
        /// A unit vector representing the direction the camera is facing
        /// </summary>
        public Vector3 Forward
        {
            get
            {
                Vector4 forward = Vector4.Transform(new Vector4(0, 0, -1, 1), Matrix.Invert(Orientation));
                return new Vector3(forward);
            }
        }

        /// <summary>
        /// A unit vector representing the direction to the right of the camera
        /// </summary>
        public Vector3 Right
        {
            get
            {
                Vector4 right = Vector4.Transform(new Vector4(1, 0, 0, 1), Matrix.Invert(Orientation));
                return new Vector3(right);
            }
        }

        /// <summary>
        /// A unit vector representing the direction out of the top of the camera
        /// </summary>
        public Vector3 Up
        {
            get
            {
                Vector4 up = Vector4.Transform(new Vector4(0, 1, 0, 1), Matrix.Invert(Orientation));
                return new Vector3(up);
            }
        }

        /// <summary>
        /// The combined camera transformation matrix, including perspective projection.
        ///
        /// This is the complete matrix to use in the vertex shader.
        /// </summary>
        public Matrix CombinedMatrix
        {
            get
            {
                return View * Projection;
            }
        }

        /// <summary>
        /// The perspective projection transformation matrix
        /// </summary>
        public Matrix Projection
        {
            get
            {
                return Matrix.CreatePerspectiveFieldOfView(_fieldOfView, _viewportAspectRatio, _nearPlane, _farPlane);
            }
        }

        /// <summary>
        /// The translation and rotation matrix of the camera.
        ///
        /// Same as the `matrix` method, except the return value does not include the projection
        /// transformation.
        /// </summary>
        public Matrix View
        {
            get
            {
                return Matrix.CreateTranslation(-_position) * Orientation;
            }
        }

        private Vector3 _position;
        private float _horizontalAngle;
        private float _verticalAngle;
        private float _fieldOfView;
        private float _nearPlane;
        private float _farPlane;
        private float _viewportAspectRatio;

        private void NormalizeAngles()
        {
            _horizontalAngle = _horizontalAngle % 360.0f;
            //% can return negative values, but this will make them all positive

            if (_horizontalAngle < 0.0f)
                _horizontalAngle += 360.0f;

            if (_verticalAngle > MaxVerticalAngle)
                _verticalAngle = MaxVerticalAngle;
            else if (_verticalAngle < -MaxVerticalAngle)
                _verticalAngle = -MaxVerticalAngle;
        }
    }
}
