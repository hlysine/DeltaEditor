# DeltaQuestionEditor-WPF

![Release build](https://github.com/Profound-Education-Centre/DeltaQuestionEditor-WPF/workflows/Release%20build/badge.svg)
![CodeQL](https://github.com/Profound-Education-Centre/DeltaQuestionEditor-WPF/workflows/CodeQL/badge.svg)

A native question editor for Windows

## Download and Install

Download the app here: [Releases](https://github.com/Profound-Education-Centre/DeltaQuestionEditor-WPF/releases/latest)

Download `Setup.exe` to install the editor. Once installed, the editor updates itself automatically when new releases are out.

## How-tos

### How to import questions and images from an Excel spreadsheet?

After starting the app, press `Import From Excel` in the welcome screen.
![Welcome screen - import from excel](https://user-images.githubusercontent.com/25472513/97781117-53e52c00-1bc4-11eb-9163-97963b6f17a9.png)

Then, select the Excel spreadsheet file that you want to import. The editor accepts `.xls`, `.xlsx` and `.xlsb` files.

Note that if the chapter you are importing contains image links, you need to put the related images together with the Excel files for the editor to find them and import them automatically.
![Pick excel file](https://user-images.githubusercontent.com/25472513/97781155-80994380-1bc4-11eb-9076-3577b1d58ca1.png)

After picking a file, the editor will ask for the chapter code. Enter the chapter that the questions in this file are related to.

Make sure that you put in the correct chapter code. The editor will use this code to handle skill codes during the import.
![Select topic of question set](https://user-images.githubusercontent.com/25472513/97781224-d1a93780-1bc4-11eb-95aa-3f834e3aea70.png)

After you press `Done`, the import process starts. The editor should look like this after importing:
![Edit screen - import successful](https://user-images.githubusercontent.com/25472513/97781300-5005d980-1bc5-11eb-8e67-068072c350b3.png)

You will also see a text file, which is a report of the import process. Read this file carefully to see if there are any unexpected errors in the import process.
![Import report](https://user-images.githubusercontent.com/25472513/97781330-86dbef80-1bc5-11eb-8814-8b1982e02678.png)

Scroll down in the import report to see the list of problems. These warn you that manual actions might be required to fix the import as the editor is not entirely sure how to handle the data correctly.
![Import report - scroll down](https://user-images.githubusercontent.com/25472513/97781364-b0951680-1bc5-11eb-8c77-cc9563a8bedb.png)

Remember to save the question set file after a successful import.

