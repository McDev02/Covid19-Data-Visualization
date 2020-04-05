# Covid19-Data-Visualization
Data visualization based on CDS - https://coronadatascraper.com/

The editor should download the latest timetable.csv automatically when you open it.

CDS has recently changed the CSV layout, once you run into errors when parsing the file, check the header of the CSV and compare it to the CSVColumns enum of the DataReaderCDS.cs
Usually it is enough to just copy paste the line (and rename "lat, long" accordingly).

Some spots such as United States are missplaced, that is in the CDS data already. You could filter the country dots out and use another way to select country level data.

Textures source: https://www.solarsystemscope.com/textures/

![Preview](https://github.com/McDev02/Covid19-Data-Visualization/blob/master/preview.jpg?raw=true)
