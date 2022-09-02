# DataTransmitter
Test project for sending and receiving files<br />
DataTransmitter consists of three projects<br />
<b>DataManagement</b> - project with path helpers, constants and file converters(classes for working with data, create headers and checksums etc.).<br />
DataManagement.Constants.ServiceBusQueueConnectionString - connection string for queue that has Max delivery count: 10, Message lock duration: 30s<br />
<b>DataSender</b> - console application for sending data to Azure Service Bus Queue and Storage Container, reads all files from selected directory and send them.<br />
<b>DataReceiver</b> - console application for receiving messages from queue. It receives message, gets data from container, sends data to server(ContentProcessor) and if everything is valid - saves file. If ContentProcessor returns error, DataReceiver validates data itself. If data is valid it will be called again.<br />

