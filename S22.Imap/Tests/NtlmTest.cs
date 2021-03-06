﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using S22.Imap.Auth.Sasl;
using S22.Imap.Auth.Sasl.Mechanisms;
using S22.Imap.Auth.Sasl.Mechanisms.Ntlm;
using System;
using System.Linq;
using System.Text;

namespace S22.Imap.Test {
	/// <summary>
	/// Contains unit tests for the NTLM Sasl mechanism.
	/// </summary>
	[TestClass]
	public class NtmlTest {
		/// <summary>
		/// Serializes an NTLM type 1 message and ensures the
		/// serialized byte array is identical to expected byte
		/// array.
		/// </summary>
		[TestMethod]
		[TestCategory("NTLM")]
		public void SerializeType1Message() {
			Type1Message msg = new Type1Message("myDomain", "myWorkstation");
			byte[] serialized = msg.Serialize();
			Assert.IsTrue(type1Message.SequenceEqual(serialized));
		}

		/// <summary>
		/// Deserializes an NTLM type 2 message and ensures the
		/// deserialized instance contains valid data.
		/// </summary>
		[TestMethod]
		[TestCategory("NTLM")]
		public void DeserializeType2Message() {
			Type2Message msg = Type2Message.Deserialize(type2MessageVersion2);

			byte[] expectedChallenge = new byte[] {
				0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef
			};
			Flags expectedFlags = Flags.NegotiateUnicode |
				Flags.NegotiateNTLM | Flags.TargetTypeDomain |
				Flags.NegotiateTargetInfo;
			Assert.AreEqual<Type2Version>(Type2Version.Version2, msg.Version);
			Assert.AreEqual<Flags>(expectedFlags, msg.Flags);
			Assert.IsTrue(expectedChallenge.SequenceEqual(msg.Challenge));
			Assert.AreEqual<long>(0, msg.Context);
			Assert.AreEqual<string>("DOMAIN", msg.TargetName);
			Assert.AreEqual<string>("DOMAIN",
				msg.TargetInformation.DomainName);
			Assert.AreEqual<string>("SERVER",
				msg.TargetInformation.ServerName);
			Assert.AreEqual<string>("domain.com",
				msg.TargetInformation.DnsDomainName);
			Assert.AreEqual<string>("server.domain.com",
				msg.TargetInformation.DnsHostname);
		}

		/// <summary>
		/// Deserializes an NTLM type 2 version 3 message and ensures the
		/// deserialized instance contains valid data.
		/// </summary>
		[TestMethod]
		[TestCategory("NTLM")]
		public void DeserializeType2Version3Message() {
			Type2Message msg = Type2Message.Deserialize(type2MessageVersion3);

			byte[] expectedChallenge = new byte[] {
				0xA6, 0xBC, 0xAF, 0x32, 0xA5, 0x51, 0x36, 0x65
			};
			Assert.AreEqual<Type2Version>(Type2Version.Version3, msg.Version);
			Assert.AreEqual<int>(42009093, (int)msg.Flags);
			Assert.IsTrue(expectedChallenge.SequenceEqual(msg.Challenge));
			Assert.AreEqual<long>(0, msg.Context);
			Assert.AreEqual<string>("LOCALHOST", msg.TargetName);
			Assert.AreEqual<string>("LOCALHOST",
				msg.TargetInformation.DomainName);
			Assert.AreEqual<string>("VMWARE-5T5GC9PU",
				msg.TargetInformation.ServerName);
			Assert.AreEqual<string>("localhost",
				msg.TargetInformation.DnsDomainName);
			Assert.AreEqual<string>("vmware-5t5gc9pu.localhost",
				msg.TargetInformation.DnsHostname);
			Assert.AreEqual<short>(3790, msg.OSVersion.BuildNumber);
			Assert.AreEqual<short>(5, msg.OSVersion.MajorVersion);
			Assert.AreEqual<short>(2, msg.OSVersion.MinorVersion);
		}

		/// <summary>
		/// Serializes an NTLM type 3 message and ensures the
		/// serialized byte array is identical to expected byte
		/// array.
		/// </summary>
		[TestMethod]
		[TestCategory("NTLM")]
		public void SerializeType3Message() {
			Type2Message m2 = Type2Message.Deserialize(type2MessageVersion3);
			// Compute the challenge response
			Type3Message msg = new Type3Message("Testuser", "Testpassword",
				m2.Challenge, "MyWorkstation");
			byte[] serialized = msg.Serialize();

			Assert.IsTrue(type3Message.SequenceEqual(serialized));
		}

		/// <summary>
		/// Verifies the various parts of a sample authentication exchange
		/// (server challenge generated by MS Exchange Server 2003).
		/// </summary>
		[TestMethod]
		[TestCategory("NTLM")]
		public void VerifyAuthenticationExchange() {
			SaslMechanism m = new SaslNtlm("TEST", "TEST");

			byte[] initialResponse = m.GetResponse(new byte[0]);
			Assert.IsTrue(initialResponse.SequenceEqual(expectedInitial));
			byte[] finalResponse = m.GetResponse(serverChallenge);
			Assert.IsTrue(finalResponse.SequenceEqual(expectedFinal));
		}

		/// <summary>
		/// Verifies the various parts of a sample authentication exchange
		/// (server challenge generated by the dovecot IMAP server).
		/// </summary>
		[TestMethod]
		[TestCategory("NTLM")]
		public void VerifyAnotherAuthenticationExchange() {
			SaslMechanism m = new SaslNtlm("test", "test");

			byte[] initialResponse = m.GetResponse(new byte[0]);
			Assert.IsTrue(initialResponse.SequenceEqual(expectedInitial));
			byte[] finalResponse = m.GetResponse(anotherServerChallenge);
			Assert.IsTrue(finalResponse.SequenceEqual(anotherExpectedFinal));
		}

		#region NTLM Type 1 message
		static byte[] type1Message = new byte[] {
			0x4E, 0x54, 0x4C, 0x4D, 0x53, 0x53, 0x50, 0x00, 0x01, 0x00, 0x00,
			0x00, 0x05, 0x32, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00, 0x28, 0x00,
			0x00, 0x00, 0x0D, 0x00, 0x0D, 0x00, 0x30, 0x00, 0x00, 0x00, 0x06,
			0x01, 0xB1, 0x1D, 0x00, 0x00, 0x00, 0x0F, 0x6D, 0x79, 0x44, 0x6F,
			0x6D, 0x61, 0x69, 0x6E, 0x6D, 0x79, 0x57, 0x6F, 0x72, 0x6B, 0x73,
			0x74, 0x61, 0x74, 0x69, 0x6F, 0x6E
		};
		#endregion

		#region NTLM Type 2 message (Version 2)
		static byte[] type2MessageVersion2 = new byte[] {
			0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x02, 0x00, 0x00,
			0x00, 0x0c, 0x00, 0x0c, 0x00, 0x30, 0x00, 0x00, 0x00, 0x01, 0x02,
			0x81, 0x00, 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x62, 0x00, 0x62, 0x00,
			0x3c, 0x00, 0x00, 0x00, 0x44, 0x00, 0x4f, 0x00, 0x4d, 0x00, 0x41,
			0x00, 0x49, 0x00, 0x4e, 0x00, 0x02, 0x00, 0x0c, 0x00, 0x44, 0x00,
			0x4f, 0x00, 0x4d, 0x00, 0x41, 0x00, 0x49, 0x00, 0x4e, 0x00, 0x01,
			0x00, 0x0c, 0x00, 0x53, 0x00, 0x45, 0x00, 0x52, 0x00, 0x56, 0x00,
			0x45, 0x00, 0x52, 0x00, 0x04, 0x00, 0x14, 0x00, 0x64, 0x00, 0x6f,
			0x00, 0x6d, 0x00, 0x61, 0x00, 0x69, 0x00, 0x6e, 0x00, 0x2e, 0x00,
			0x63, 0x00, 0x6f, 0x00, 0x6d, 0x00, 0x03, 0x00, 0x22, 0x00, 0x73,
			0x00, 0x65, 0x00, 0x72, 0x00, 0x76, 0x00, 0x65, 0x00, 0x72, 0x00,
			0x2e, 0x00, 0x64, 0x00, 0x6f, 0x00, 0x6d, 0x00, 0x61, 0x00, 0x69,
			0x00, 0x6e, 0x00, 0x2e, 0x00, 0x63, 0x00, 0x6f, 0x00, 0x6d, 0x00,
			0x00, 0x00, 0x00, 0x00
		};
		#endregion

		#region NTLM Type 2 message (Version 3)
		static byte[] type2MessageVersion3 = new byte[] {
			0x4E, 0x54, 0x4C, 0x4D, 0x53, 0x53, 0x50, 0x00, 0x02, 0x00, 0x00,
			0x00, 0x12, 0x00, 0x12, 0x00, 0x38, 0x00, 0x00, 0x00, 0x05, 0x02,
			0x81, 0x02, 0xA6, 0xBC, 0xAF, 0x32, 0xA5, 0x51, 0x36, 0x65, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x9E, 0x00, 0x9E, 0x00,
			0x4A, 0x00, 0x00, 0x00, 0x05, 0x02, 0xCE, 0x0E, 0x00, 0x00, 0x00,
			0x0F, 0x4C, 0x00, 0x4F, 0x00, 0x43, 0x00, 0x41, 0x00, 0x4C, 0x00,
			0x48, 0x00, 0x4F, 0x00, 0x53, 0x00, 0x54, 0x00, 0x02, 0x00, 0x12,
			0x00, 0x4C, 0x00, 0x4F, 0x00, 0x43, 0x00, 0x41, 0x00, 0x4C, 0x00,
			0x48, 0x00, 0x4F, 0x00, 0x53, 0x00, 0x54, 0x00, 0x01, 0x00, 0x1E,
			0x00, 0x56, 0x00, 0x4D, 0x00, 0x57, 0x00, 0x41, 0x00, 0x52, 0x00,
			0x45, 0x00, 0x2D, 0x00, 0x35, 0x00, 0x54, 0x00, 0x35, 0x00, 0x47,
			0x00, 0x43, 0x00, 0x39, 0x00, 0x50, 0x00, 0x55, 0x00, 0x04, 0x00,
			0x12, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x63, 0x00, 0x61, 0x00, 0x6C,
			0x00, 0x68, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x74, 0x00, 0x03, 0x00,
			0x32, 0x00, 0x76, 0x00, 0x6D, 0x00, 0x77, 0x00, 0x61, 0x00, 0x72,
			0x00, 0x65, 0x00, 0x2D, 0x00, 0x35, 0x00, 0x74, 0x00, 0x35, 0x00,
			0x67, 0x00, 0x63, 0x00, 0x39, 0x00, 0x70, 0x00, 0x75, 0x00, 0x2E,
			0x00, 0x6C, 0x00, 0x6F, 0x00, 0x63, 0x00, 0x61, 0x00, 0x6C, 0x00,
			0x68, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x74, 0x00, 0x05, 0x00, 0x12,
			0x00, 0x6C, 0x00, 0x6F, 0x00, 0x63, 0x00, 0x61, 0x00, 0x6C, 0x00,
			0x68, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x74, 0x00, 0x00, 0x00, 0x00,
			0x00
		};
		#endregion

		#region NTLM Type 3 message
		static byte[] type3Message = new byte[] {			
			0x4E, 0x54, 0x4C, 0x4D, 0x53, 0x53, 0x50, 0x00, 0x03, 0x00, 0x00,
			0x00, 0x18, 0x00, 0x18, 0x00, 0x48, 0x00, 0x00, 0x00, 0x18, 0x00,
			0x18, 0x00, 0x60, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x78,
			0x00, 0x00, 0x00, 0x10, 0x00, 0x10, 0x00, 0x78, 0x00, 0x00, 0x00,
			0x1A, 0x00, 0x1A, 0x00, 0x88, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0xA2, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01,
			0xB1, 0x1D, 0x00, 0x00, 0x00, 0x0F, 0xF6, 0x0C, 0x93, 0x17, 0x97,
			0x1C, 0x44, 0x9A, 0xAF, 0xBF, 0xC6, 0xD9, 0x44, 0xC9, 0x06, 0x2E,
			0x47, 0x6F, 0xCD, 0x57, 0xBC, 0x42, 0xD2, 0xEC, 0xBC, 0x85, 0xC7,
			0x73, 0x00, 0xAA, 0x9F, 0xEB, 0x6A, 0xF3, 0x02, 0x6C, 0xF7, 0x91,
			0x8D, 0x15, 0xF3, 0xE2, 0xB3, 0x84, 0xDE, 0x46, 0xBE, 0xDB, 0x54,
			0x00, 0x65, 0x00, 0x73, 0x00, 0x74, 0x00, 0x75, 0x00, 0x73, 0x00,
			0x65, 0x00, 0x72, 0x00, 0x4D, 0x00, 0x79, 0x00, 0x57, 0x00, 0x6F,
			0x00, 0x72, 0x00, 0x6B, 0x00, 0x73, 0x00, 0x74, 0x00, 0x61, 0x00,
			0x74, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00	 
		};
		#endregion

		#region Authentication Exchange
		static byte[] expectedInitial = new byte[] {
			0x4E, 0x54, 0x4C, 0x4D, 0x53, 0x53, 0x50, 0x00, 0x01, 0x00, 0x00,
			0x00, 0x05, 0x32, 0x00, 0x00, 0x06, 0x00, 0x06, 0x00, 0x28, 0x00,
			0x00, 0x00, 0x0B, 0x00, 0x0B, 0x00, 0x2E, 0x00, 0x00, 0x00, 0x06,
			0x01, 0xB1, 0x1D, 0x00, 0x00, 0x00, 0x0F, 0x64, 0x6F, 0x6D, 0x61,
			0x69, 0x6E, 0x77, 0x6F, 0x72, 0x6B, 0x73, 0x74, 0x61, 0x74, 0x69,
			0x6F, 0x6E
		};

		static byte[] serverChallenge = new byte[] {
			0x4E, 0x54, 0x4C, 0x4D, 0x53, 0x53, 0x50, 0x00, 0x02, 0x00, 0x00,
			0x00, 0x12, 0x00, 0x12, 0x00, 0x38, 0x00, 0x00, 0x00, 0x05, 0x02,
			0x81, 0x02, 0x82, 0x9F, 0x92, 0xC1, 0x22, 0x63, 0x99, 0x02, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x9E, 0x00, 0x9E, 0x00,
			0x4A, 0x00, 0x00, 0x00, 0x05, 0x02, 0xCE, 0x0E, 0x00, 0x00, 0x00,
			0x0F, 0x4C, 0x00, 0x4F, 0x00, 0x43, 0x00, 0x41, 0x00, 0x4C, 0x00,
			0x48, 0x00, 0x4F, 0x00, 0x53, 0x00, 0x54, 0x00, 0x02, 0x00, 0x12,
			0x00, 0x4C, 0x00, 0x4F, 0x00, 0x43, 0x00, 0x41, 0x00, 0x4C, 0x00,
			0x48, 0x00, 0x4F, 0x00, 0x53, 0x00, 0x54, 0x00, 0x01, 0x00, 0x1E,
			0x00, 0x56, 0x00, 0x4D, 0x00, 0x57, 0x00, 0x41, 0x00, 0x52, 0x00,
			0x45, 0x00, 0x2D, 0x00, 0x35, 0x00, 0x54, 0x00, 0x35, 0x00, 0x47,
			0x00, 0x43, 0x00, 0x39, 0x00, 0x50, 0x00, 0x55, 0x00, 0x04, 0x00,
			0x12, 0x00, 0x6C, 0x00, 0x6F, 0x00, 0x63, 0x00, 0x61, 0x00, 0x6C,
			0x00, 0x68, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x74, 0x00, 0x03, 0x00,
			0x32, 0x00, 0x76, 0x00, 0x6D, 0x00, 0x77, 0x00, 0x61, 0x00, 0x72,
			0x00, 0x65, 0x00, 0x2D, 0x00, 0x35, 0x00, 0x74, 0x00, 0x35, 0x00,
			0x67, 0x00, 0x63, 0x00, 0x39, 0x00, 0x70, 0x00, 0x75, 0x00, 0x2E,
			0x00, 0x6C, 0x00, 0x6F, 0x00, 0x63, 0x00, 0x61, 0x00, 0x6C, 0x00,
			0x68, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x74, 0x00, 0x05, 0x00, 0x12,
			0x00, 0x6C, 0x00, 0x6F, 0x00, 0x63, 0x00, 0x61, 0x00, 0x6C, 0x00,
			0x68, 0x00, 0x6F, 0x00, 0x73, 0x00, 0x74, 0x00, 0x00, 0x00, 0x00,
			0x00
		};

		static byte[] expectedFinal = new byte[] {
			0x4E, 0x54, 0x4C, 0x4D, 0x53, 0x53, 0x50, 0x00, 0x03, 0x00, 0x00,
			0x00, 0x18, 0x00, 0x18, 0x00, 0x48, 0x00, 0x00, 0x00, 0x18, 0x00,
			0x18, 0x00, 0x60, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x78,
			0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00, 0x78, 0x00, 0x00, 0x00,
			0x16, 0x00, 0x16, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x96, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01,
			0xB1, 0x1D, 0x00, 0x00, 0x00, 0x0F, 0x16, 0xF6, 0xC6, 0x49, 0x65,
			0xD6, 0x73, 0x14, 0x50, 0xD1, 0x52, 0x56, 0x43, 0x94, 0x04, 0x8B,
			0x89, 0xCD, 0xEF, 0x41, 0x22, 0x75, 0x1A, 0x4E, 0x50, 0xEF, 0x89,
			0x1C, 0x1D, 0x8E, 0xAC, 0x10, 0xDD, 0xED, 0x7C, 0x35, 0xE5, 0x62,
			0xC8, 0x75, 0x75, 0x5E, 0x10, 0xA5, 0x43, 0x44, 0x26, 0x70, 0x54,
			0x00, 0x45, 0x00, 0x53, 0x00, 0x54, 0x00, 0x57, 0x00, 0x6F, 0x00,
			0x72, 0x00, 0x6B, 0x00, 0x73, 0x00, 0x74, 0x00, 0x61, 0x00, 0x74,
			0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00
		};
		#endregion

		#region Another Authentication Exchange
		static byte[] anotherServerChallenge = new byte[] {
			0x4E, 0x54, 0x4C, 0x4D, 0x53, 0x53, 0x50, 0x00, 0x02, 0x00, 0x00,
			0x00, 0x0C, 0x00, 0x0C, 0x00, 0x30, 0x00, 0x00, 0x00, 0x05, 0x02,
			0x82, 0x00, 0x78, 0x35, 0x52, 0x30, 0xD2, 0xCA, 0xD9, 0xB8, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x14, 0x00, 0x14, 0x00,
			0x3C, 0x00, 0x00, 0x00, 0x64, 0x00, 0x65, 0x00, 0x62, 0x00, 0x69,
			0x00, 0x61, 0x00, 0x6E, 0x00, 0x03, 0x00, 0x0C, 0x00, 0x64, 0x00,
			0x65, 0x00, 0x62, 0x00, 0x69, 0x00, 0x61, 0x00, 0x6E, 0x00, 0x00,
			0x00, 0x00, 0x00
		};

		static byte[] anotherExpectedFinal = new byte[] {
			0x4E, 0x54, 0x4C, 0x4D, 0x53, 0x53, 0x50, 0x00, 0x03, 0x00, 0x00,
			0x00, 0x18, 0x00, 0x18, 0x00, 0x48, 0x00, 0x00, 0x00, 0x18, 0x00,
			0x18, 0x00, 0x60, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x78,
			0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00, 0x78, 0x00, 0x00, 0x00,
			0x16, 0x00, 0x16, 0x00, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
			0x00, 0x96, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x06, 0x01,
			0xB1, 0x1D, 0x00, 0x00, 0x00, 0x0F, 0x4E, 0xE9, 0xDD, 0x7B, 0x84,
			0x62, 0x62, 0x67, 0x64, 0xD2, 0xD2, 0x11, 0xC3, 0xEF, 0xD3, 0xC1,
			0x32, 0x35, 0x15, 0xB8, 0x34, 0xAB, 0x95, 0xFD, 0x3D, 0xD2, 0xAA,
			0x04, 0xC2, 0x6D, 0x11, 0xEC, 0x3E, 0x22, 0xE4, 0x25, 0x73, 0x18,
			0xBB, 0x3D, 0x1E, 0x45, 0x52, 0xA1, 0x39, 0xB7, 0x66, 0x3B, 0x74,
			0x00, 0x65, 0x00, 0x73, 0x00, 0x74, 0x00, 0x57, 0x00, 0x6F, 0x00,
			0x72, 0x00, 0x6B, 0x00, 0x73, 0x00, 0x74, 0x00, 0x61, 0x00, 0x74,
			0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00
		};
		#endregion
	}
}