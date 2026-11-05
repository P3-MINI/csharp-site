"""Prints daily temperature statistics from data/measurements.csv."""
import csv
from collections import defaultdict
from pathlib import Path

DATA = Path(__file__).parent.parent / "data" / "measurements.csv"


def main() -> None:
    days = defaultdict(list)
    with DATA.open(newline="") as file:
        for row in csv.DictReader(file):
            day = row["timestamp"][:10]
            days[day].append(float(row["temperature"]))
    for day, values in sorted(days.items()):
        average = sum(values) / len(values)
        print(f"{day}: min {min(values):5.1f}  max {max(values):5.1f}  avg {average:5.1f}")


if __name__ == "__main__":
    main()
