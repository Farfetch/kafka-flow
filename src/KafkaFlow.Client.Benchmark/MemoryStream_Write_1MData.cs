// namespace KafkaFlow.Client.Benchmark
// {
//     using System;
//     using System.IO;
//     using BenchmarkDotNet.Attributes;
//     using KafkaFlow.Client.Protocol.Streams;
//
//     public class MemoryStream_Write_1MData
//     {
//         private const int N = 1024 * 1024;
//         private readonly byte[] source;
//
//         private DynamicMemoryStream dynamic;
//         private StaticMemoryStream stc;
//         private MemoryStream common;
//
//         public MemoryStream_Write_1MData()
//         {
//             this.source = new byte[N];
//             new Random(DateTime.Now.Second).NextBytes(this.source);
//         }
//
//         [IterationSetup]
//         public void Setup()
//         {
//             this.stc = new StaticMemoryStream(MemoryManager.Instance, this.source.Length);
//             this.dynamic = new DynamicMemoryStream(MemoryManager.Instance, 1024 * 8);
//             this.common = new MemoryStream();
//         }
//
//         [IterationCleanup]
//         public void Clean()
//         {
//             this.dynamic.Dispose();
//             this.stc.Dispose();
//         }
//
//         [Benchmark]
//         public void Static()
//         {
//             this.stc.Write(this.source);
//         }
//
//         [Benchmark]
//         public void Dynamic()
//         {
//             this.dynamic.Write(this.source);
//         }
//
//         [Benchmark]
//         public void Common()
//         {
//             this.common.Write(this.source);
//         }
//     }
// }