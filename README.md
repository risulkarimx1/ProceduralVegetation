# Project Background: 
Replicar Simulator has the feature to create the road on runtime from map data. As part of this feature, the procedural vegetation generator was developed. The purpose of this project was to create natural-looking vegetation with very low run-time to generate them.
# Implementation approach:
## Step1: Creating Vegetation Placement Areas
The road data contains points along which the road is generated. From each of the points, on each side of the road, a rectangle area is created. All these rectangles act as placement area. Rectangles are created with specific rotation to match the curve of the road.

## Step2: Creating Blue Noise inside the rectangle
Inside this rectangle, based on the density parameter set from the settings of the vegetation system. Here point disk sampling algorithm is used to create the noise map. To make the system performant, C# job systems is used with burst compiler. 

## Step3: Finding vegetation place on terrain
Based on the blue noise created from step 2, a RaycastCommandJob is used to find out points on Terrain's surface. For each of the positions, a tree is selected from a list of tree instances mentioned in the Vegetation Setup file.

For details object (grasses) similar steps are followed. 

Full source code of the project was not give as I worked on the porject owned by my employer. The project is temporarily hosted for my portfolio purpose. Risul Karim, 20.01.2020
