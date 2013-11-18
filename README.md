netmfazurestorage
=================

A unified Windows Azure Storage SDK for [.net Microframework](http://netmf.codeplex.com/) v4.2.

## Get Started

Install the following components:

* [Microsoft Visual C# Express 2010](http://www.microsoft.com/express/downloads/#2010-Visual-CS)
* [.NET Micro Framework SDK v4.2](http://www.netduino.com/downloads/MicroFrameworkSDK_NETMF42_QFE2.msi)
* [Netduino SDK v4.2.2.0 (32-bit)](http://www.netduino.com/downloads/netduinosdk_32bit_NETMF42.exe) or [Netduino SDK v4.2.2.0 (64-bit)](http://www.netduino.com/downloads/netduinosdk_64bit_NETMF42.exe)

## Working with Tables

Create an instance of `TableClient` with your storage account and key:

```cs
var client = new TableClient(new CloudStorageAccount(account,key));
```

The client can be used to create a table:

```cs
client.CreateTable("helloworld");
```

To insert new entities, create a `Hashtable` with the values you wish to store:

```cs
var values = new Hashtable();
values.Add("guidfield", Guid.NewGuid());
values.Add("int32field", 32);
values.Add("stringfield", "string");
values.Add("doublefield", (double)123.22);
values.Add("int64field", (Int64)64);
values.Add("boolfield", true);

client.InsertTableEntity("helloworld", "PK", "RK", DateTime.Now, values);
```

You can query for a single entity, which will return a single `Hashtable` containing all the entity properties:

```cs
var entity = client.QueryTable("helloworld", "PK", "RK");
```

Or you can pass in an OData query, and get an array of Hashtables back:

```cs
var entities = client.QueryTable("netmftest", "PartitionKey eq '2'");
```
