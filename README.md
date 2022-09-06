# DataTransmitter
Test project for sending and receiving files<br />
DataTransmitter consists of three projects<br />
<b>DataManagement</b> - project with senders and receivers, path helpers, constants and file converters(classes for working with data, create headers and checksums etc.).<br />
<b>DataSender</b> - console application for sending data to Azure Service Bus Queue and Storage Container, reads all files from selected directory and send them.<br />
<b>DataReceiver</b> - console application for receiving messages from queue. It receives message, gets data from container, sends data to server(ContentProcessor) and if everything is valid - saves file. If ContentProcessor returns error, DataReceiver validates data itself. Valid data will be called again. Invalid message will be sent to dead queue and then will be deleted, file will be deleted from BLOB container.<br />
<b>Processor v1</b> - folder with 3rd party application that do some work with file content.<br />

