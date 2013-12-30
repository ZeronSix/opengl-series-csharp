using System;
using System.Collections.Generic;
using Pencil.Gaming.Graphics;

namespace tdogl
{
    /// <summary>
    /// Represents an OpenGL program made by linking shaders.
    /// </summary>
    public class Program: IDisposable
    {
        /// <summary>
        /// Creates a program by linking a list of tdogl::Shader objects
        /// </summary>
        /// <param name="shaders">The <see cref="tdogl.Shader">shaders</see> to link together to make the program.</param>
        public Program(List<Shader> shaders)
        {
            _object = 0;
            if (shaders.Count < 1)
                throw new Exception("No shaders were provided to create the program");

            // create the program object
            _object = GL.CreateProgram();
            if (_object == 0)
                throw new Exception("glCreateProgram failed");

            // attach all the shaders
            foreach (Shader shader in shaders)
                GL.AttachShader(_object, shader.GlObject);

            GL.LinkProgram(_object);

            // detach all the shaders
            foreach (Shader shader in shaders)
                GL.DetachShader(_object, shader.GlObject);

            // throw exception if linking failed
            int status;
            GL.GetProgram(_object, ProgramParameter.LinkStatus, out status);
            if (status == 0) {
                string msg = "Program linking failure: ";

                string strInfoLog;
                GL.GetProgramInfoLog((int)_object, out strInfoLog);
                msg += strInfoLog;

                GL.DeleteProgram(_object);
                _object = 0;
                throw new Exception(msg);
            }
        }

        public void Dispose()
        {
            if (_object != 0)
                GL.DeleteProgram(_object);
        }

        /// <summary>
        /// The program's object ID, as returned from glCreateProgram
        /// </summary>
        public uint GlObject
        {
            get { return _object; }
        }

        /// <returns>The attribute index for the given name, as returned from glGetAttribLocation.</returns>
        public int Attrib(string attribName)
        {
            if (attribName == null)
                throw new Exception("attribName was NULL");

            int attrib = GL.GetAttribLocation(_object, attribName);
            if (attrib == -1)
                throw new Exception("Program attribute not found: " + attribName);

            return attrib;
        }

        /// <returns>The uniform index for the given name, as returned from glGetUniformLocation.</returns>
        public int Uniform(string uniformName)
        {
            if (uniformName == null)
                throw new Exception("uniformName was NULL");

            int uniform = GL.GetUniformLocation(_object, uniformName);
            if (uniform == -1)
                throw new Exception("Program attribute not found: " + uniformName);

            return uniform;
        }

        private uint _object;
    }
}
