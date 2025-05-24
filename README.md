# SDF-Fractal-Dithering
A reimplementation of Runevision's Fractal Dithering that uses a 2D SDF instead of lookup textures. This allows for using shapes other than a circle as the dots and removes the memory requirement of storing a precomputed lookup table.

![Preview](https://github.com/mattdevv/SDF-Fractal-Dithering/blob/main/Images/shapes.png?raw=true)

## Instructions
Add the folder `Fractal Dithering` to your Unity project and use the included Material or Shader. Control to change color/shape/size are on the Material. Tested in Unity 6 (6000.0.38f1).

## Limitations
My implementation does not have most of the variations of the original and as such is not ready for use. The shader needs more options to control surface details such as specular highlights.

Many quality improvements are missing too such as the variable SDF edge width, and a lookup table for converting luminance to shape size.

## Acknowledgements
Thanks to Rune for the inspiring video that started it all. https://www.youtube.com/watch?v=HPqGaIMVuLs
