# DataTransmitter
Test project for sending and receiving files
DataTransmitter consists of three projects
DataManagement - project with path helpers, constants and file converters(classes for working with data, create headers and checksums etc.)
DataSender - console application for sending data to Azure Service Bus Queue and Storage Container, reads all files from selected directory and send them.
DataReceiver - console application for receiving messages from queue. It receives message, gets data from container, sends data to server(ContentProcessor) and if everything is valid - saves file. If ContentProcessor returns error, DataReceiver validates data itself. If data is valid it will be called again.
