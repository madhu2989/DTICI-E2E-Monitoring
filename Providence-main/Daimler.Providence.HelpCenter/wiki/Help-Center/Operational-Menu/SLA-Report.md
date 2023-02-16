# SLA Report
Using the SLA Reports you can monitor the compliance of the Service Level Agreement, and you can recognize incidents early.

**Common example of a availability and outages:**
A good overview gives the following table showing the cumulative availability of a service per year in percent. 
The calculation is made on following basis: 365 days * 24 hours/day = 8.760 hours / year

|Availability Percentage|Availability (Year)|Downtime (Year)|
|--|--|--|
|100 %|≙ 8.760 h|≙ 0 h|
|99 %|≙ 8.672 h|≙ 88 h|
|90 %|≙ 7.884 h|≙ 876 h|
|80 %|≙ 7.008 h|≙ 1.752 h|
|50 %|≙ 4.380 h|≙ 4.380 h|
|10 %|≙ 876 h|≙  7.884‬ h|

---
**Calculation basis**
Our calculations for the SLA-report are not calculated on the complete data, it will be calculated along a defined time range.
Also at first you need to define the time duration in which you want to have the SLA report. This time period is named "SLA-Period".

**Downtime** ≙ sum(Error-Duration) + sum(Warning-Duration) in SLA-period

**Uptime** ≙ 1- (Downtime/SLA-period) [%]

**Sample:**

**SLA-period:** 01.01.2020 - 02.01.2020 -> SLA-period: 24 [h]

**Error:** 01.01.2020 05:00 - 01.01.2020 6:00 -> Error-Duration: 1 [h]

**Warning:** 01.01.2020 07:00 - 01.01.2020 8:00 -> Warning-Duration: 1 [h]

Downtime = (1 + 1) = 2 [h]

Uptime = 1 - (2 /24) =  0,91 [%]

---

## SLA Report Overview
In order to retrieve the SLA calculation for a specifiy environment you have to specify:
- The environment name of your environment
- The start and end date of the calculation

**Note:** If the start and end date of the SLA period is not given we take as default the last 3 days for the calculation.

The calculations are based on 24 hours / day.

![SLA_Report_Creation.png](.attachments/SLA_Report_Creation.png)

After starting a SLA calculation the queued job is shown in the SLA Jobs Overview

![SLA_Job_Overview.png](.attachments/SLA_Job_Overview.png)

A queued job can have one of the following states:
- Queued
- Running
- Processed
- Error

## SLA Report Evaluation

After the SLA is finished and the state was updated to "Processed" you can click on the "Show data" button to open the SLA Overview.

![SLA_Report_Overview.png](.attachments/SLA_Report_Overview.png)

Here you can select the representation type and optionally a single element you want to retriev the SLA calculation for.

![SLA_Report_Overview_Properties.png](.attachments/SLA_Report_Overview2021-03-30_15-45-05.png)

After selecting those properties you can click on "Show Report" to show the calculated SLA values

![SLA_Report_Values1.png](.attachments/SLA_Report_Values_2021-03-30_15-45-16.png)

The "SLA State" delivers the state of the corresponding element and can contain "Error", "Warning" and "OK".
The corresponding value is shown as percent or as a chart (depends on representation format).
If no SLA data is available this is shown in the field "ComponentType" as "No SLA Data found".

## SLA Representation formats

- **"Value" representation format**
  The SLA value is always given in percent and correlates to the SLA uptime.

  ![SLA_Report_Values2.png](.attachments/SLA_Report_Values_2021-03-30_15-45-16.png)

- **"Pie Chart" representation format**
  The SLA Pie correlates also always to the SLA Uptime.

  ![SLA_Report_Piex.png](.attachments/SLA_Report_Values_Pie_2021-03-30_15-45-16.png)

  - <FONT COLOR=#006633>**Green part** = Element was **..% available**</FONT>
  - <FONT COLOR=#FF9933>**Yellow part** = Element was with a **..% Warning**</FONT>
  - <FONT COLOR=#FF0000> **Red part** = Element was with a **..% Error**</FONT>

  **Mouse double click** - on a corresponding pie shows a detail information of the data. 

  ![SLA_Report_Pie_1.png](.attachments/SLA_Report_Pie_1-df19ea37-0c0c-4a65-b4ea-8cf08a48e6b9.png)
  ![SLA_Report_Pie_2.png](.attachments/SLA_Report_Pie_2-ab006dfe-7a5f-4956-bad9-6519b7dedae6.png)

- **"Line Chart" representation format**
  The SLA line chart correlates also on the SLA uptime.

  ![SLA_Line_Chartx.png](.attachments/SLA_Report_Values_Line_2021-03-30_15-45-16.png)

  **Mouse double click** - on a corresponding line chart shows a detail information of the data. 

  ![SLA_Line_Chart1.png](.attachments/SLA_Line_Chart1-1b4d47de-4183-4d91-ae94-7cce872def2a.png)
  ![SLA_Line_Chart2.png](.attachments/SLA_Line_Chart2-14eab595-8f8b-4e7d-9e79-eb439e00723a.png)
