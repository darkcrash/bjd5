using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bjd.Common.Test;
using Bjd.DnsServer;
using Xunit;

namespace DnsServerTest{
    public class PacketDnsHeaderTest{

        //[C#]
        //private string str0 = "000381800001000200030004";
        private string str0 = "000381800001000200030004";

        [Fact]
        public void getClsの確認(){
            //setUp
            PacketDnsHeader sut = new PacketDnsHeader(TestUtil.HexStream2Bytes(str0), 0);
            ushort expected = 0x0003;
            //exercise
            ushort actual = sut.Id;
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void getFlagsの確認(){
            //setUp
            PacketDnsHeader sut = new PacketDnsHeader(TestUtil.HexStream2Bytes(str0), 0);
            ushort expected =  0x8180;
            //exercise
            ushort actual = sut.Flags;
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void getQDの確認(){
            //setUp
            PacketDnsHeader sut = new PacketDnsHeader(TestUtil.HexStream2Bytes(str0), 0);
            ushort expected = 1;
            //exercise
            ushort actual = sut.GetCount(0); //QD=0
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void getANの確認(){
            //setUp
            PacketDnsHeader sut = new PacketDnsHeader(TestUtil.HexStream2Bytes(str0), 0);
            ushort expected = 2;
            //exercise
            ushort actual = sut.GetCount(1); //AN=1
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void getNSの確認(){
            //setUp
            PacketDnsHeader sut = new PacketDnsHeader(TestUtil.HexStream2Bytes(str0), 0);
            ushort expected = 3;
            //exercise
            ushort actual = sut.GetCount(2); //NS=2
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void getARの確認(){
            //setUp
            PacketDnsHeader sut = new PacketDnsHeader(TestUtil.HexStream2Bytes(str0), 0);
            ushort expected = 4;
            //exercise
            ushort actual = sut.GetCount(3); //AR=3
            //verify
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void setCountの確認(){
            //setUp
            PacketDnsHeader sut = new PacketDnsHeader();
            var expected = (ushort)0xf1f1;
            sut.SetCount(3, expected);
            //exercise
            ushort actual = sut.GetCount(3);
            //verify
            Assert.Equal(expected, actual);
        }
    }
}