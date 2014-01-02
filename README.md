# About

This is a C# port of the code from a series of OpenGL articles on http://tomdalling.com/
Port is based on a great library called Pencil.Gaming (https://github.com/antonijn/Pencil.Gaming)

__WARNING: Although all articles have been ported and they are working, this port is still WIP!__
__There is a lot of things in this port that needs to be improved.__

List of articles that are already ported:

 1. [Getting Started in Xcode, Visual C++, and Linux](http://tomdalling.com/blog/modern-opengl/01-getting-started-in-xcode-and-visual-cpp/)
 2. [Textures](http://tomdalling.com/blog/modern-opengl/02-textures/)
 3. [Matrices, Depth Buffering, Animation](http://tomdalling.com/blog/modern-opengl/03-matrices-depth-buffering-animation/)
 4. [Cameras, Vectors & Input](http://tomdalling.com/blog/modern-opengl/04-cameras-vectors-and-input/)
 5. [Model Assets & Instances](http://tomdalling.com/blog/modern-opengl/05-model-assets-and-instances/)
 6. [Diffuse Point Lighting](http://tomdalling.com/blog/modern-opengl/06-diffuse-point-lighting/)
 7. [More Lighting: Ambient, Specular, Attenuation, Gamma](http://tomdalling.com/blog/modern-opengl/07-more-lighting-ambient-specular-attenuation-gamma/)

VS2013 Solution is located at build\vs2013\
Original C++/Objective-C code is [here](https://github.com/tomdalling/opengl-series).

# Things that i need to do
- Linux and Mac OS X support via Mono.
- Android and iOS support via Xamarin.Android and Xamarin.iOS.
- Do something with GLEW.
- tdogl framework is not fully ported. Only things that source code needs to be compiled are ported.
- Fix some bugs
- Add solution files for VS2010 and MonoDevelop 4 (Xamarin Studio).

# Credits

Thanks to Tom Dalling for creating these articles. 

Thanks to Antonie Blom for creating Pencil.Gaming.

# License

Licensed under the Apache License, Version 2.0. See LICENSE.txt.
