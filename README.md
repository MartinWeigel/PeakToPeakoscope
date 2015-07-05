# PeakToPeakoscope

PeakToPeakoscope is a pipeline for the [Pico Technology](https://www.picotech.com) oscilloscopes (PS6000). It calculates and filters the peak-to-peak data of up to four channels. For example, it can be used for touch sensing by measuring the mutual capacitance between two electrodes. The data is send to other applications using the [OSC protocol](http://opensoundcontrol.org). Therefore, applications can be programmed in any OSC-compatible programming language and do not need to include interfacing and filtering of the oscilloscope data directly.

The pipeline currently contains two filters, super sampling and a box filter. *Super sampling* uses *n* peak-to-peak values and averages them. Only the averaged value is processed and send over OSC to avoid clogging the application. Before sending it, the value can be further filtered further with a *box filter* over the last *n* supersampled values to smooth the output.

The pipeline was initially used to demonstrate the capacitive touch sensing of [iSkin](http://www.MartinWeigel.com/iskin.html). It was tested with *Picoscope 6402A*. The transmitted square signal was generated using an Agilent 33210A wave generator (>5V, 1.000 kHz). An example application written in Java/Processing can be found in the [iSkin Music Demo](https://github.com/MartinWeigel/iSkinMusicDemo).

## OSC Message
Default OSC address is "127.0.0.1" on port 5500. The message is named "/PeakToPeak" and contains three int values for better cross-library support than long and double values. The values contain:
 1. the timestamp
 2. the channel [0-3]
 3. the peak-to-peak value of the channel

## License
All files written by myself can be used according to the MIT license. Check the header of the files you plan to use for more information.

PeakToPeakoscope is based on the example C# program by [Pico Technology](https://www.picotech.com). Please respect the following terms for all Pico Technology related files (i.e. Main.cs, PS6000Imports.cs, and PS6000PinnedArray.cs):
> The example programs in the SDK may be modified, copied and distributed for the purpose of developing programs to collect data using Pico products.

## Acknowledgements
This software was programmed as part of my work in the [Embodied Interaction Group](http://embodied.mpi-inf.mpg.de) at the [Max-Planck Institute for Informatics](http://www.mpi-inf.mpg.de), the [Cluster of Excellence MMCI](http://www.mmci.uni-saarland.de) and [Saarland University](http://www.uni-saarland.de). It has partially been funded by the Cluster of Excellence on Multimodal Computing and Interaction within the German Federal Excellence Initiative.
