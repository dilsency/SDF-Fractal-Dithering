# SDF-Fractal-Dithering
A reimplementation of Runevision's Fractal Dithering that uses a 2D SDF instead of lookup textures. This allows for using shapes other than a circle as the dots and removes the memory requirement of storing a precomputed lookup table.

## Instructions
Add the folder `Fractal Dithering` to your Unity project and use the included Material or Shader. Control to change color/shape/size are on the Material. Requires URP, tested in Unity 6 (6000.0.38f1).

![Preview](https://github.com/mattdevv/SDF-Fractal-Dithering/blob/main/Images/shapes.png?raw=true)

![Debug View](https://github.com/mattdevv/SDF-Fractal-Dithering/blob/main/Images/debug.png?raw=true)

## Limitations
My implementation does not have most of the variations of the original and as such is not ready for use. The shader needs more options to control surface details such as specular highlights.

Can still add improvements such as RGB color and a lookup table for converting luminance to shape radius.

## Acknowledgements
Thanks to Rune for the inspiring video that started it all. https://www.youtube.com/watch?v=HPqGaIMVuLs
See more details and the original implementation here: https://github.com/runevision/Dither3D
