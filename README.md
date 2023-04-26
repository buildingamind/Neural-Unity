![Alt Text](https://i.imgur.com/VeXOLcd.gif)

Continuous Time Recurrent Neural Network implemented in the Unity Game Engine.

**USAGE:**
* Start a Unity Project and execute the CTRNN.unitypackage to import and generate the filestructure within your project.*
* Neurons can be instanitated through the prefab, or added as a component to an existing GameObject with CTRNN.cs.


**SCRIPTS:**
* CTRNN.cs : *Main CTRNN class. Ensure that you have Gizmos enabled in the Unity Editor for the script to perform some magic behaviors.*
* Signal.cs : *Implements an artificial signal input to the network.*
* OutputFromNeuron.cs : *This is an optional neuron component which converts a Neuron Output Value into a behavior. Placed on the Neuron GameObject.*
* ReadFromNeuron.cs : *This is an optional object component which converts a Neuron Output Value into a behavior. Placed on the Target GameObject itself.*
* BaseSensor.cs : *Base Class which implements a simple public CTRNN target neuron, value output, and sustain parameter.*
* VisionSensor.cs : *An example subclass of the Sensor Class which generates a RayCast to set the Value.*
* DistanceSensor.cs : *A simple sensor which reads the distance from the object it is placed on to a target, and sends that value to a neuron.*
* DirectionSensor.cs : *A simple sensor which returns magnetic north (simulated & defined as Vector3.forward) according to the agent.*

**Python Notebook:**
* Plot_CTRNN_Data.ipynb: *Notebook provided for extracting and plotting data from networks whose values are exported.*
* 

**SCENES:**
* Demo 1 - Neurons and Networks.unity : *This scene shows a simple 1 Input / 4 Hidden / 1 Output Neuron*
* Demo 2 - Hungry Agents.unity : *This scene shows simple environment where Neurons recieve a VisionSensor Input and feed towards two Output neurons.*
* Demo 3 - Boxing.unity : *This scene shows simple environment where Neurons recieve a VisionSensor Input + TouchSensor and feed towards three Output neurons.*
* Demo 4 - Resevoir Networks: *This scene uses the BuildRingNetwork script to generate a ring-like resevoir network with a central signal neuron.*

**PREFABS:**
* CTRNN.prefab : *This is a simple Neuron Prefab which contains optional Signal and Output components. (You may leave these components disabled if the neurons are hidden.)*
(As of now, the prefabs do not work well with GitHub due to Unity's serialization structure, so it's advised to either create a new Sphere GameObject with the CTRNN script attached (and Signal/Output if necessary.) or use the CTRNN Unity Package to setup the filestructure first. (Advised)
* Several other demo prefabs exist in the Prefabs/Demo Prefabs/, which are a combination of CTRNN, Bodies, and Sensors.

**Unity Packages:**
1. CTRNN.unitypackage : *This is a Unity Package which contains each of the following scripts. This is arguably easier to use than placing the raw files in your Unity project, but also is liable to not get updates as often because it is a built pacakage. To use this file, open any Unity Scene and execute the .unitypackage file. It will ask you to import the contents, which you will. You should do this only if you have trouble using the raw code. You may also be able to first unpack the unitypackage and then point your GitHub repo to clone towards the new directory created.

Other Useful Information:
* Sustain is the amount that a neuron retains its value each time step. This is the case mainly for sensors whose value is not always controlled by a network and whose value you want to be continuous rather than 0 or 1. value = value * sustain. A reasonable value for sustain is betwee 0 and < 1. A sustain value of 1 will leave a neuron permanently active once it is activated.

