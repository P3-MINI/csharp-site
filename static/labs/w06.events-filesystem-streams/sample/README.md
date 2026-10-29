# Weather Station

A small project collecting measurements from a home weather station.
The station reports temperature, humidity and air pressure every ten
minutes and stores the readings in `data/measurements.csv`.

## Layout

- `data/` - raw measurements and the station configuration,
- `docs/` - user guide and the list of changes,
- `media/` - pictures used in the documentation,
- `scripts/` - helper scripts for generating reports.

## Quick start

1. Copy `data/config.json` and adjust the station name and location.
2. Connect the sensors and start the logger.
3. Run `python scripts/report.py` to print daily statistics.

Temporary files (`*.tmp`), copies of old versions (`*.bak`) and the
`cache/` directory do not need to be backed up.
