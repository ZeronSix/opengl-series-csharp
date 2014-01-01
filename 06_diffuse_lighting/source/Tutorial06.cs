/*
 main
 
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
using System.Collections.Generic;
using Pencil.Gaming;
using Pencil.Gaming.Graphics;
using Pencil.Gaming.MathUtils;
using tdogl;

namespace opengl_series
{
    /// <summary>
    /// Represents a textured geometry asset
    ///
    /// Contains everything necessary to draw arbitrary geometry with a single texture:
    ///
    /// - shaders
    /// - a texture
    /// - a VBO
    /// - a VAO
    /// - the parameters to glDrawArrays (drawType, drawStart, drawCount)
    /// </summary>
    public class ModelAsset
    {
        public Program Shaders;
        public Texture Texture;
        public uint Vbo;
        public uint Vao;
        public BeginMode DrawType;
        public int DrawStart;
        public int DrawCount;

        public ModelAsset()
        {
            Vbo = 0;
            Vao = 0;
            DrawType = BeginMode.Triangles;
            DrawStart = 0;
            DrawCount = 0;
        }
    }

    public class ModelInstance
    {
        public ModelAsset Asset;
        public Matrix Transform;

        public ModelInstance()
        {
            Transform = new Matrix();
        }
    }
    
    public struct Light
    {
        public Vector3 Position;
        public Vector3 Intensities; // a.k.a. the color of the light
    }

    public class MainClass
    {
        private static readonly Vector2i ScreenSize = new Vector2i(800, 600);

        private static GlfwWindowPtr _window;
        private static Camera _gCamera = new Camera();
        private static float _gDegreesRotated = 0.0f;
        private static ModelAsset _gWoodenCrate;
        private static Light _gLight;
        private static readonly List<ModelInstance> _gInstances = new List<ModelInstance>();

        // returns a new tdogl.Program created from the given vertex and fragment shader filenames
        private static Program LoadShaders(string vertFilename, string fragFilename)
        {
            List<Shader> shaders = new List<Shader>();
            shaders.Add(Shader.ShaderFromFile(vertFilename, ShaderType.VertexShader));
            shaders.Add(Shader.ShaderFromFile(fragFilename, ShaderType.FragmentShader));
            return new Program(shaders);
        }

        // returns a new tdogl.Texture created from the given filename
        private static Texture LoadTexture(string filename)
        {
            Bitmap bmp = Bitmap.LoadFromFile(filename);
            bmp.FlipVertically();
            return new Texture(bmp);
        }

        // initialises the _gWoodenCrate global
        private static void LoadWoodenCrateAsset()
        {
            // set all the elements of gWoodenCrate
            _gWoodenCrate = new ModelAsset();
            _gWoodenCrate.Shaders = LoadShaders("vertex-shader.glsl", "fragment-shader.glsl");
            _gWoodenCrate.DrawType = BeginMode.Triangles;
            _gWoodenCrate.DrawStart = 0;
            _gWoodenCrate.DrawCount = 6 * 2 * 3;
            _gWoodenCrate.Texture = LoadTexture("wooden-crate.png");
            GL.GenBuffers(1, out _gWoodenCrate.Vbo);
            GL.GenVertexArrays(1, out _gWoodenCrate.Vao);

            // bind the VAO
            GL.BindVertexArray(_gWoodenCrate.Vao);

            // bind the VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, _gWoodenCrate.Vbo);

            // Make a cube out of triangles (two triangles per side)
            float[] vertexData = new float[] {
                //  X     Y     Z       U     V          Normal
                // bottom
                -1.0f,-1.0f,-1.0f,   0.0f, 0.0f,   0.0f, -1.0f, 0.0f,
                 1.0f,-1.0f,-1.0f,   1.0f, 0.0f,   0.0f, -1.0f, 0.0f,
                -1.0f,-1.0f, 1.0f,   0.0f, 1.0f,   0.0f, -1.0f, 0.0f,
                 1.0f,-1.0f,-1.0f,   1.0f, 0.0f,   0.0f, -1.0f, 0.0f,
                 1.0f,-1.0f, 1.0f,   1.0f, 1.0f,   0.0f, -1.0f, 0.0f,
                -1.0f,-1.0f, 1.0f,   0.0f, 1.0f,   0.0f, -1.0f, 0.0f,

                // top
                -1.0f, 1.0f,-1.0f,   0.0f, 0.0f,   0.0f, 1.0f, 0.0f,
                -1.0f, 1.0f, 1.0f,   0.0f, 1.0f,   0.0f, 1.0f, 0.0f,
                 1.0f, 1.0f,-1.0f,   1.0f, 0.0f,   0.0f, 1.0f, 0.0f,
                 1.0f, 1.0f,-1.0f,   1.0f, 0.0f,   0.0f, 1.0f, 0.0f,
                -1.0f, 1.0f, 1.0f,   0.0f, 1.0f,   0.0f, 1.0f, 0.0f,
                 1.0f, 1.0f, 1.0f,   1.0f, 1.0f,   0.0f, 1.0f, 0.0f,

                // front
                -1.0f,-1.0f, 1.0f,   1.0f, 0.0f,   0.0f, 0.0f, 1.0f,
                 1.0f,-1.0f, 1.0f,   0.0f, 0.0f,   0.0f, 0.0f, 1.0f,
                -1.0f, 1.0f, 1.0f,   1.0f, 1.0f,   0.0f, 0.0f, 1.0f,
                 1.0f,-1.0f, 1.0f,   0.0f, 0.0f,   0.0f, 0.0f, 1.0f,
                 1.0f, 1.0f, 1.0f,   0.0f, 1.0f,   0.0f, 0.0f, 1.0f,
                -1.0f, 1.0f, 1.0f,   1.0f, 1.0f,   0.0f, 0.0f, 1.0f,

                // back
                -1.0f,-1.0f,-1.0f,   0.0f, 0.0f,   0.0f, 0.0f, -1.0f,
                -1.0f, 1.0f,-1.0f,   0.0f, 1.0f,   0.0f, 0.0f, -1.0f,
                 1.0f,-1.0f,-1.0f,   1.0f, 0.0f,   0.0f, 0.0f, -1.0f,
                 1.0f,-1.0f,-1.0f,   1.0f, 0.0f,   0.0f, 0.0f, -1.0f,
                -1.0f, 1.0f,-1.0f,   0.0f, 1.0f,   0.0f, 0.0f, -1.0f,
                 1.0f, 1.0f,-1.0f,   1.0f, 1.0f,   0.0f, 0.0f, -1.0f,

                // left
                -1.0f,-1.0f, 1.0f,   0.0f, 1.0f,   -1.0f, 0.0f, 0.0f,
                -1.0f, 1.0f,-1.0f,   1.0f, 0.0f,   -1.0f, 0.0f, 0.0f,
                -1.0f,-1.0f,-1.0f,   0.0f, 0.0f,   -1.0f, 0.0f, 0.0f,
                -1.0f,-1.0f, 1.0f,   0.0f, 1.0f,   -1.0f, 0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f,   1.0f, 1.0f,   -1.0f, 0.0f, 0.0f,
                -1.0f, 1.0f,-1.0f,   1.0f, 0.0f,   -1.0f, 0.0f, 0.0f,

                // right
                 1.0f,-1.0f, 1.0f,   1.0f, 1.0f,   1.0f, 0.0f, 0.0f,
                 1.0f,-1.0f,-1.0f,   1.0f, 0.0f,   1.0f, 0.0f, 0.0f,
                 1.0f, 1.0f,-1.0f,   0.0f, 0.0f,   1.0f, 0.0f, 0.0f,
                 1.0f,-1.0f, 1.0f,   1.0f, 1.0f,   1.0f, 0.0f, 0.0f,
                 1.0f, 1.0f,-1.0f,   0.0f, 0.0f,   1.0f, 0.0f, 0.0f,
                 1.0f, 1.0f, 1.0f,   0.0f, 1.0f,   1.0f, 0.0f, 0.0f
            };
            GL.BufferData<float>(BufferTarget.ArrayBuffer, new IntPtr(vertexData.Length * sizeof(float)), vertexData, BufferUsageHint.StaticDraw);

            // connect the xyz to the "vert" attribute of the vertex shader
            GL.EnableVertexAttribArray(_gWoodenCrate.Shaders.Attrib("vert"));
            GL.VertexAttribPointer(_gWoodenCrate.Shaders.Attrib("vert"), 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            // connect the uv coords to the "vertTexCoord" attribute of the vertex shader
            GL.EnableVertexAttribArray(_gWoodenCrate.Shaders.Attrib("vertTexCoord"));
            GL.VertexAttribPointer(_gWoodenCrate.Shaders.Attrib("vertTexCoord"), 2, VertexAttribPointerType.Float, true, 8 * sizeof(float), 3 * sizeof(float));

            // connect the normal to the "vertNormal" attribute of the vertex shader
            GL.EnableVertexAttribArray(_gWoodenCrate.Shaders.Attrib("vertNormal"));
            GL.VertexAttribPointer(_gWoodenCrate.Shaders.Attrib("vertNormal"), 3, VertexAttribPointerType.Float, true, 8 * sizeof(float), 5 * sizeof(float));

            // unbind the VAO
            GL.BindVertexArray(0);
        }

        // convenience function that returns a translation matrix
        private static Matrix Translate(float x, float y, float z)
        {
            return Matrix.CreateTranslation(new Vector3(x, y, z));
        }

        // convenience function that returns a scaling matrix
        private static Matrix Scale(float x, float y, float z)
        {
            return Matrix.CreateScale(new Vector3(x, y, z));
        }

        //create all the `instance` structs for the 3D scene, and add them to `gInstances`
        private static void CreateInstances()
        {
            ModelInstance dot = new ModelInstance();
            dot.Asset = _gWoodenCrate;
            dot.Transform = new Matrix();
            _gInstances.Add(dot);

            ModelInstance i = new ModelInstance();
            i.Asset = _gWoodenCrate;
            i.Transform = Scale(1, 2, 1) * Translate(0, -4, 0);
            _gInstances.Add(i);

            ModelInstance hLeft = new ModelInstance();
            hLeft.Asset = _gWoodenCrate;
            hLeft.Transform = Scale(1, 6, 1) * Translate(-8, 0, 0);
            _gInstances.Add(hLeft);

            ModelInstance hRight = new ModelInstance();
            hRight.Asset = _gWoodenCrate;
            hRight.Transform = Scale(1, 6, 1) * Translate(-4, 0, 0);
            _gInstances.Add(hRight);

            ModelInstance hMid = new ModelInstance();
            hMid.Asset = _gWoodenCrate;
            hMid.Transform = Scale(2, 1, 0.8f) * Translate(-6, 0, 0);
            _gInstances.Add(hMid);
        }

        private static void RenderInstance(ModelInstance inst)
        {
            ModelAsset asset = inst.Asset;
            Program shaders = asset.Shaders;

            // bind the shaders
            shaders.Use();

            // set the shader uniforms
            Matrix m = _gCamera.CombinedMatrix;
            shaders.SetUniform("camera", ref m);
            shaders.SetUniform("model", ref inst.Transform);
            shaders.SetUniform("tex", 0); // set to 0 because the texture will be bound to GL_TEXTURE0
            shaders.SetUniform("light.position", _gLight.Position);
            shaders.SetUniform("light.intensities", _gLight.Intensities);

            //bind the texture
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, asset.Texture.GLObject);

            // bind VAO and draw
            GL.BindVertexArray(asset.Vao);
            GL.DrawArrays(asset.DrawType, asset.DrawStart, asset.DrawCount);

            // unbind everything
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            shaders.StopUsing();
        }

        // draws a single frame
        private static void Render()
        {
            // clear everything
            GL.ClearColor(0, 0, 0, 1); // black
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            foreach (ModelInstance inst in _gInstances) {
                RenderInstance(inst);
            }

            // update events
            Glfw.PollEvents();

            // swap the display buffers (displays what was just drawn)
            Glfw.SwapBuffers(_window);
        }

        // update the scene based on the time elapsed since last update
        private static void Update(float secondsElapsed)
        {
            //rotate the first instance in `gInstances`
            float degreesPerSecond = 180.0f;
            _gDegreesRotated += secondsElapsed * degreesPerSecond;
            while (_gDegreesRotated > 360.0f) _gDegreesRotated -= 360.0f;
            _gInstances[0].Transform = Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), _gDegreesRotated);

            //move position of camera based on WASD keys, and XZ keys for up and down
            float moveSpeed = 4.0f; //units per second

            if (Glfw.GetKey(_window, Key.S)) {
                _gCamera.OffsetPosition((float)secondsElapsed * moveSpeed * -_gCamera.Forward);
            }
            else if (Glfw.GetKey(_window, Key.W)) {
                _gCamera.OffsetPosition((float)secondsElapsed * moveSpeed * _gCamera.Forward);
            }
            if (Glfw.GetKey(_window, Key.A)) {
                _gCamera.OffsetPosition((float)secondsElapsed * moveSpeed * -_gCamera.Right);
            }
            else if (Glfw.GetKey(_window, Key.D)) {
                _gCamera.OffsetPosition((float)secondsElapsed * moveSpeed * _gCamera.Right);
            }
            if (Glfw.GetKey(_window, Key.Z)) {
                _gCamera.OffsetPosition((float)secondsElapsed * moveSpeed * -new Vector3(0, 1, 0));
            }
            else if (Glfw.GetKey(_window, Key.X)) {
                _gCamera.OffsetPosition((float)secondsElapsed * moveSpeed * new Vector3(0, 1, 0));
            }

            if (Glfw.GetKey(_window, Key.One))
                _gLight.Position = _gCamera.Position;

            if (Glfw.GetKey(_window, Key.Two))
                _gLight.Intensities = new Vector3(1, 0, 0); // red
            else if (Glfw.GetKey(_window, Key.Three))
                _gLight.Intensities = new Vector3(0, 1, 0); //green
            else if (Glfw.GetKey(_window, Key.Four))
                _gLight.Intensities = new Vector3(1, 1, 1); //white

            //rotate camera based on mouse movement
            float mouseSensitivity = 0.1f;
            double mouseX, mouseY;
            Glfw.GetCursorPos(_window, out mouseX, out mouseY);
            _gCamera.OffsetOrientation(mouseSensitivity * (float)mouseY, mouseSensitivity * (float)mouseX);
            Glfw.SetCursorPos(_window, 0, 0); //reset the mouse, so it doesn't go out of the window
        } 

        // the pragram starts here
        private static void AppMain()
        {
            // initialize GLFW
            if (!Glfw.Init()) 
                throw new Exception("glfwInit failed");

            // open a window with GLFW
            Glfw.WindowHint(WindowHint.OpenGLProfile, (int)OpenGLProfile.Core);
            Glfw.WindowHint(WindowHint.ContextVersionMajor, 3);
            Glfw.WindowHint(WindowHint.ContextVersionMinor, 2);
            _window = Glfw.CreateWindow(ScreenSize.X, ScreenSize.Y, "", GlfwMonitorPtr.Null, GlfwWindowPtr.Null);
            if (_window.Equals(GlfwWindowPtr.Null))
                throw new Exception("glfwOpenWindow failed. Can your hardware handle OpenGL 3.2?");
            Glfw.MakeContextCurrent(_window);

            // GLFW settings
            Glfw.SetInputMode(_window, InputMode.CursorMode, CursorMode.CursorHidden | CursorMode.CursorCaptured);
            Glfw.SetCursorPos(_window, 0, 0);
            Glfw.SetScrollCallback(_window, new GlfwScrollFun((win, x, y) => {
                //increase or decrease field of view based on mouse wheel
                float zoomSensitivity = -0.2f;
                float fieldOfView = _gCamera.FieldOfView + zoomSensitivity * (float)y;
                if (fieldOfView < 5.0f) fieldOfView = 5.0f;
                if (fieldOfView > 130.0f) fieldOfView = 130.0f;
                _gCamera.FieldOfView = fieldOfView;
            }));

            // TODO: GLEW in C#

            // print out some info about the graphics drivers
            Console.WriteLine("OpenGL version: {0}", GL.GetString(StringName.Version));
            Console.WriteLine("GLSL version: {0}", GL.GetString(StringName.ShadingLanguageVersion));
            Console.WriteLine("Vendor: {0}", GL.GetString(StringName.Vendor));
            Console.WriteLine("Renderer: {0}", GL.GetString(StringName.Renderer));

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            // initialise the gWoodenCrate asset
            LoadWoodenCrateAsset();

            // create all the instances in the 3D scene based on the gWoodenCrate asset
            CreateInstances();

            _gCamera.Position = new Vector3(-4, 0, 17);
            _gCamera.ViewportAspectRatio = (float)ScreenSize.X / (float)ScreenSize.Y;
            _gCamera.SetNearAndFarPlanes(0.5f, 100.0f);

            // setup light
            _gLight = new Light() {
                Position = _gCamera.Position,
                Intensities = new Vector3(1, 1, 1) // white
            };

            float lastTime = (float)Glfw.GetTime();
            // run while window is open
            while (!Glfw.WindowShouldClose(_window)) {
                // update the scene based on the time elapsed since last update
                float thisTime = (float)Glfw.GetTime();
                Update(thisTime - lastTime);
                lastTime = thisTime;

                // draw one frame
                Render();
                //exit program if escape key is pressed
                if (Glfw.GetKey(_window, Key.Escape))
                    Glfw.SetWindowShouldClose(_window, true);
            }

            // clean up and exit
            Glfw.Terminate();
        }

        public static void Main(string[] args)
        {
            try {
                AppMain();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                Environment.Exit(1);
            }
        }
    }
}
