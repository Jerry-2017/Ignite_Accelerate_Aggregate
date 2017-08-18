# Ignite_Accelerate_Aggregate

## What

This is Software Engineer internship program among 3 months of Suzhou, Microsoft. The project aim to improve perfomance of U-SQL query from aggregation of databases.

## How

Currently , Caching is the first option in our solution.


Hi , everyone this is the conclusion of ignite performance test

Some Issues:

    if you want to repeat this experience, follow following step will save your time.

    Ignite Environment Setting Procedure:

        1. Download and unzip Ignite.Net Binary Package
        2. Install Java Development Kit (correponding version)
        3. Setting IGNITE_HOME to Ignite Package path without trailing "\"
        4. Setting JAVA_HOME to Java Development kit
        5. Copy and reference shared library in C#, in ./platforms/dotnet/bin/

    Iginte Appliance:

        1. Ignition.Start([config]) or Ignition.StartFromApplication() load content in app.config 
        2. Ignition.ClientMode = true for client node
        3. Network Configuration:
            Using TcpDiscoveryStaticIpFinder with form of Ip:47500..47509
            Choose correct local Ip address of given adapter!
            Ensure Client and Server of given IP are bi-directionally inter-connected 

    SQL and Cache Grid:
        1. A class is needed to identify  a relational table
        2. Ignite will call BinarySerialize to compress class into bits, add [Serializable] to class or implement IBinarizable 
        3. You need to add [QuerySqlField] to field you want to query and [QuerySqlField(IsIndexed = true)] to index them.	Pay attention , it must add on the field not property!
        4. Datastreamer would accelerate Cache loading.
        5. A 3rd party class in ignite supports backend SQL DB as persistent storage.
        6. SQL supports durable storage in disk and recovery from failure.

Performance Result:

	Ignite:

	Read/Write thread   Concurrent RW Latency(ms)   Only Read Latency(ms)
	1			        1.3583625653584			    1.21158337225596 
	2			        2.24588572485207		    2.0436455977463
	4			        4.12084323815992		    4.29124409386553

	Ignite client consume too much cpu load and test can only continue to this extent, Server load < 10% during this process , ignite have automatic query thread with quantity of cpu cores, and it could be the bottleneck.

	The Performance is not good compared OLTP, the problem is that Ignite is not native SQL engine, it's developed from its cache grid and lack of efficiency thus.