# LCD Simulator

A simulator for the Hitachi HD44780 LCD controller chip, designed to behave nearly identically given the same inputs.

- [LCD Simulator](#lcd-simulator)
  - [Features](#features)
  - [Using the GUI](#using-the-gui)
  - [Using the CLI](#using-the-cli)
    - [Alone](#alone)
    - [With the GUI](#with-the-gui)
  - [Controlling from other applications](#controlling-from-other-applications)
  - [Screenshots](#screenshots)
  - [Links](#links)

## Features

- Full simulation of all read and write operations
  - Operations mimic how real controllers behave, even in undefined circumstances
  - Includes support for the 8 custom characters
- Compatible will all modes of operation
  - 4/8-bit interface
  - 1/2-line display mode
  - 5x8/11 font
- Accurate simulator display using the same font as a real controller
  - Adjustable contrast that changes depending on the display mode
  - Customisable display size and colours
- Controllable by manually toggling pins, selecting from a list of instructions, or through a CLI
- A character selector grid that lists all possible characters for writing
- Optional logging of all operations done to the display

## Using the GUI

The GUI is launched by running `LCDSimulator.GUI.exe`. After launching, the display must first be powered on by clicking on the VDD pin, which will then turn green to signify that it is powered. Clicking any of the other non-ground pins (with the exception of VO) will also toggle them on and off. Click on the A pin to toggle the simulated backlight, then use the mouse scroll wheel while hovering the pointer over the VO pin to adjust the display contrast.

To interface with the display, you can either toggle each pin manually, or select an instruction from the instruction list to the side. Operations are executed by turning the E pin on then off again, as with a real display. If the Address Counter is currently in DDRAM and you select the "Write Data" operation, a window will open with every possible character that can be written to the screen to select from.

The size of the screen, as well as its colour, can be changed from the `Screen` menu.

## Using the CLI

The command-line interface is very similar to that used by my Raspberry Pi Pico [`uart_lcd` application](https://github.com/TollyH/raspberry-pi-pico/tree/main/uart_lcd) that interacts with real displays, with most of the commands being identical.

The CLI can either be used on its own, or to interact with the display shown on the GUI.

### Alone

The CLI can be launched on its own with `LCDSimulator.CLI.exe`. The simulation will run the same as it would through the GUI, however there will be no visual feedback to any of the operations.

You must use the `#power 1` command to turn on the display before you can interact with it. This is the same as enabling the VDD pin on the GUI.

### With the GUI

To use the CLI to control the display shown on the GUI, open the GUI's terminal window by clicking on `Options > Show console window`. You can then issue commands through the CLI, which will then be reflected on the display visualisation.

The terminal window can also be used to log every operation done on the display by launching the GUI with the `--log` command-line parameter. This cannot be toggled once the application is launched.

## Controlling from other applications

The simulator can also be controlled from other programs by piping the standard input and output streams of the application. The interface is identical to that of the regular CLI, and the commands will behave the same as they would when being controlled through a terminal.

The GUI will respond to commands on the standard input stream even when the terminal window is not visible. The GUI's standard error stream is used for the logging feature, so should not be redirected, as it is not part of the CLI.

These are some examples of programs that interface with the GUI:

- [lcd_video_sim.py](https://gist.github.com/TollyH/81289034d05e208bde14c8726710a311#file-lcd_video_sim-py) - A version of the [lcd_video.py](https://gist.github.com/TollyH/81289034d05e208bde14c8726710a311#file-lcd_video-py) script that instead interfaces with the simulator
- [LCD Remote Controller](https://github.com/TollyH/LCD_Remote_Controller/tree/simulator) - The `simulator` branch of the controller C# app contains a modified version that interfaces with the simulator

## Screenshots

![A 20x4 display](https://github.com/TollyH/LCDSimulator/blob/main/.github/README%20Images/GUI-Main-1.png?raw=true)
*A 20x4 display*

![A 16x2 display](https://github.com/TollyH/LCDSimulator/blob/main/.github/README%20Images/GUI-Main-2.png?raw=true)
*A 16x2 display*

![5x11 font](https://github.com/TollyH/LCDSimulator/blob/main/.github/README%20Images/GUI-Main-3.png?raw=true)
*5x11 font*

![20x4 display in 1-line mode](https://github.com/TollyH/LCDSimulator/blob/main/.github/README%20Images/GUI-Main-4.png?raw=true)
*20x4 display in 1-line mode*

![Custom characters](https://github.com/TollyH/LCDSimulator/blob/main/.github/README%20Images/GUI-Main-5.png?raw=true)
*Custom characters*

![Character selector](https://github.com/TollyH/LCDSimulator/blob/main/.github/README%20Images/GUI-Characters-1.png?raw=true)
*Character selector*

![Available sizes](https://github.com/TollyH/LCDSimulator/blob/main/.github/README%20Images/GUI-ScreenOps-1.png?raw=true)
*Available sizes*

![Available colours](https://github.com/TollyH/LCDSimulator/blob/main/.github/README%20Images/GUI-ScreenOps-2.png?raw=true)
*Available colours*

## Links

- [Hitachi HD44780 specification](https://www.sparkfun.com/datasheets/LCD/HD44780.pdf)
