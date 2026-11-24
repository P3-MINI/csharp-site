# User Guide

This guide describes how to install, configure and maintain the weather
station. It assumes that the sensors are already connected to the board
and that the board runs the logger service.

## Installation

The logger is a single service started automatically when the board boots.
It reads the configuration file, opens the sensors and starts writing the
measurements to a CSV file. Every line of the file contains a timestamp,
the sensor identifier and the measured values.

To install the service, copy the project directory to the board and run the
installation script with administrator rights. The script registers the
service, creates the data directory and checks that all sensors respond.
If a sensor does not respond, the script prints its identifier and stops.

## Configuration

The configuration is stored in `data/config.json`. The most important
settings are:

- `station` - the name of the station shown in the reports,
- `location` - geographical coordinates of the station,
- `intervalMinutes` - how often the measurements are taken,
- `sensors` - the list of enabled sensors.

After changing the configuration, restart the logger service. The service
validates the file on start and refuses to run if a setting is invalid.
In that case the reason is written to the system log.

## Reading the data

The measurements are stored in a plain CSV file, so they can be opened in
any spreadsheet or processed with a script. The first line is a header.
Values use a dot as the decimal separator regardless of the system locale,
so the file can be shared between computers with different settings.

The helper script `scripts/report.py` reads the file and prints the daily
minimum, maximum and average temperature. It does not modify the data.

## Maintenance

- Clean the sensors every few months; dust affects the humidity readings.
- Keep a backup of the `data/` directory. The CSV file grows by roughly
  one kilobyte per hour, so a year of measurements fits in a few megabytes.
- Check the clock of the board after a power failure. Wrong timestamps
  make the daily statistics useless.

## Troubleshooting

If the report shows no data for a day, check whether the logger service
was running and whether the disk is full. If the values are clearly wrong
(for example a temperature of -40 degrees in summer), the sensor is most
likely disconnected. Reconnect it and restart the service.
