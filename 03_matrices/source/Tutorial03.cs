﻿/*
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
    public class MainClass
    {
        private static readonly Vector2i ScreenSize = new Vector2i(800, 600);
        private static GlfwWindowPtr _window;
        private static Texture _gTexture;
        private static Program _gProgram;
        private static uint gVAO = 0, gVBO = 0;
        private static float _gDegreesRotated = 0.0f;

        // loads the vertex shader and fragment shader, and links them to make the global gProgram
        private static void LoadShaders()
        {
            List<Shader> shaders = new List<Shader>();
            shaders.Add(Shader.ShaderFromFile("vertex-shader.glsl", ShaderType.VertexShader));
            shaders.Add(Shader.ShaderFromFile("fragment-shader.glsl", ShaderType.FragmentShader));
            _gProgram = new Program(shaders);

            _gProgram.Use();

            // set the "projection" uniform in the vertex shader, because it's not going to change
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(50.0f), (float)ScreenSize.X / (float)ScreenSize.Y, 0.1f, 10.0f);
            // Matrix projection = Matrix.CreateOrthographicOffCenter(-2, 2, -2, 2, 0.1f, 10);
            _gProgram.SetUniform("projection", ref projection);

            // set the "camera" uniform in the vertex shader, because it's also not going to change
            Matrix camera = Matrix.LookAt(new Vector3(3, 3, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            _gProgram.SetUniform("camera", ref camera);

            _gProgram.StopUsing();
        }

        // loads a triangle into the VAO global
        private static void LoadTriangle()
        {
            // make and bind the VAO
            GL.GenVertexArrays(1, out gVAO);
            GL.BindVertexArray(gVAO);

            // make and bind the VBO
            GL.GenBuffers(1, out gVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, gVBO);

            // Put the three triangle verticies into the VBO
            float[] vertexData = new float[] {
                //  X     Y     Z       U     V
                // bottom
                -1.0f,-1.0f,-1.0f,   0.0f, 0.0f,
                 1.0f,-1.0f,-1.0f,   1.0f, 0.0f,
                -1.0f,-1.0f, 1.0f,   0.0f, 1.0f,
                 1.0f,-1.0f,-1.0f,   1.0f, 0.0f,
                 1.0f,-1.0f, 1.0f,   1.0f, 1.0f,
                -1.0f,-1.0f, 1.0f,   0.0f, 1.0f,

                // top
                -1.0f, 1.0f,-1.0f,   0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f,   0.0f, 1.0f,
                 1.0f, 1.0f,-1.0f,   1.0f, 0.0f,
                 1.0f, 1.0f,-1.0f,   1.0f, 0.0f,
                -1.0f, 1.0f, 1.0f,   0.0f, 1.0f,
                 1.0f, 1.0f, 1.0f,   1.0f, 1.0f,

                // front
                -1.0f,-1.0f, 1.0f,   1.0f, 0.0f,
                 1.0f,-1.0f, 1.0f,   0.0f, 0.0f,
                -1.0f, 1.0f, 1.0f,   1.0f, 1.0f,
                 1.0f,-1.0f, 1.0f,   0.0f, 0.0f,
                 1.0f, 1.0f, 1.0f,   0.0f, 1.0f,
                -1.0f, 1.0f, 1.0f,   1.0f, 1.0f,

                // back
                -1.0f,-1.0f,-1.0f,   0.0f, 0.0f,
                -1.0f, 1.0f,-1.0f,   0.0f, 1.0f,
                 1.0f,-1.0f,-1.0f,   1.0f, 0.0f,
                 1.0f,-1.0f,-1.0f,   1.0f, 0.0f,
                -1.0f, 1.0f,-1.0f,   0.0f, 1.0f,
                 1.0f, 1.0f,-1.0f,   1.0f, 1.0f,

                // left
                -1.0f,-1.0f, 1.0f,   0.0f, 1.0f,
                -1.0f, 1.0f,-1.0f,   1.0f, 0.0f,
                -1.0f,-1.0f,-1.0f,   0.0f, 0.0f,
                -1.0f,-1.0f, 1.0f,   0.0f, 1.0f,
                -1.0f, 1.0f, 1.0f,   1.0f, 1.0f,
                -1.0f, 1.0f,-1.0f,   1.0f, 0.0f,

                // right
                 1.0f,-1.0f, 1.0f,   1.0f, 1.0f,
                 1.0f,-1.0f,-1.0f,   1.0f, 0.0f,
                 1.0f, 1.0f,-1.0f,   0.0f, 0.0f,
                 1.0f,-1.0f, 1.0f,   1.0f, 1.0f,
                 1.0f, 1.0f,-1.0f,   0.0f, 0.0f,
                 1.0f, 1.0f, 1.0f,   0.0f, 1.0f
            };
            GL.BufferData<float>(BufferTarget.ArrayBuffer, new IntPtr(vertexData.Length * sizeof(float)), vertexData, BufferUsageHint.StaticDraw);

            // connect the xyz to the "vert" attribute of the vertex shader
            GL.EnableVertexAttribArray(_gProgram.Attrib("vert"));
            GL.VertexAttribPointer(_gProgram.Attrib("vert"), 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            // connect the uv coords to the "vertTexCoord" attribute of the vertex shader
            GL.EnableVertexAttribArray(_gProgram.Attrib("vertTexCoord"));
            GL.VertexAttribPointer(_gProgram.Attrib("vertTexCoord"), 2, VertexAttribPointerType.Float, true, 5 * sizeof(float), 3 * sizeof(float));

            // unbind the VBO and VAO
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        // loads the file "hazard.png" into gTexture
        private static void LoadTexture()
        {
            Bitmap bmp = Bitmap.LoadFromFile("wooden-crate.png");
            bmp.FlipVertically();
            _gTexture = new Texture(bmp);
        }

        // draws a single frame
        private static void Render()
        {
            // clear everything
            GL.ClearColor(0, 0, 0, 1); // black
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // bind the program (the shaders)
            _gProgram.Use();

            // set the "model" uniform in the vertex shader, based on the gDegreesRotated global
            Matrix model = Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), MathHelper.ToRadians(_gDegreesRotated));
            _gProgram.SetUniform("model", ref model);

            // bind the texture and set the "tex" uniform in the fragment shader
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, _gTexture.GLObject);
            _gProgram.SetUniform("tex", 0);

            // bind the VAO (the triangle)
            GL.BindVertexArray(gVAO);

            // draw the VAO
            GL.DrawArrays(BeginMode.Triangles, 0, 6 * 2 * 3);

            // unbind the VAO, the program and the texture
            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            _gProgram.StopUsing();

            // update events
            Glfw.PollEvents();

            // swap the display buffers (displays what was just drawn)
            Glfw.SwapBuffers(_window);
        }

        // update the scene based on the time elapsed since last update
        private static void Update(double secondsElapsed)
        {
            //rotate by 1 degree
            float degreesPerSecond = 180.0f;
            _gDegreesRotated += (float)secondsElapsed * degreesPerSecond;

            //don't go over 360 degrees
            while (_gDegreesRotated > 360.0f) _gDegreesRotated -= 360.0f;
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

            // TODO: GLEW in C#

            // print out some info about the graphics drivers
            Console.WriteLine("OpenGL version: {0}", GL.GetString(StringName.Version));
            Console.WriteLine("GLSL version: {0}", GL.GetString(StringName.ShadingLanguageVersion));
            Console.WriteLine("Vendor: {0}", GL.GetString(StringName.Vendor));
            Console.WriteLine("Renderer: {0}", GL.GetString(StringName.Renderer));

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            // load vertex and fragment shaders into opengl
            LoadShaders();

            // load the texture
            LoadTexture();

            // create buffer and fill it with the points of the triangle
            LoadTriangle();

            double lastTime = Glfw.GetTime();
            // run while window is open
            while (!Glfw.WindowShouldClose(_window)) {
                // update the scene based on the time elapsed since last update
                double thisTime = Glfw.GetTime();
                Update(thisTime - lastTime);
                lastTime = thisTime;

                // draw one frame
                Render();
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
                Environment.Exit(1);
            }
        }
    }
}
