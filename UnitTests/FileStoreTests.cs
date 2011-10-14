﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    class FileStoreTests
    {
        QuickFix.FileStore store;
        QuickFix.FileStoreFactory factory;

        QuickFix.SessionSettings settings;
        QuickFix.SessionID sessionID;

        [SetUp]
        public void setup()
        {
            if (System.IO.Directory.Exists("store"))
                System.IO.Directory.Delete("store", true);

            sessionID = new QuickFix.SessionID("FIX.4.2", "SENDERCOMP", "TARGETCOMP");

            QuickFix.Dictionary config = new QuickFix.Dictionary();
            config.SetString(QuickFix.SessionSettings.CONNECTION_TYPE, "initiator");
            config.SetString(QuickFix.SessionSettings.FILE_STORE_PATH, "store");

            settings = new QuickFix.SessionSettings();
            settings.Set(sessionID, config);
            factory = new QuickFix.FileStoreFactory(settings);

            store = (QuickFix.FileStore)factory.Create(sessionID);
        }

        void rebuildStore()
        {
            if(store != null)
            {
                store.Dispose();
            }

            store = (QuickFix.FileStore)factory.Create(sessionID);
        }


        [TearDown]
        public void teardown()
        {
            store.Dispose();
        }

        [Test]
        public void generateFileNamesTest()
        {
            Assert.That(System.IO.File.Exists("store/FIX.4.2-SENDERCOMP-TARGETCOMP.seqnums"));
            Assert.That(System.IO.File.Exists("store/FIX.4.2-SENDERCOMP-TARGETCOMP.body"));
            Assert.That(System.IO.File.Exists("store/FIX.4.2-SENDERCOMP-TARGETCOMP.header"));
        }

        [Test]
        public void getNextSenderMsgSeqNumTest()
        {
            Assert.AreEqual(1, store.GetNextSenderMsgSeqNum());
            store.SetNextSenderMsgSeqNum(5);
            Assert.AreEqual(5, store.GetNextSenderMsgSeqNum());
            rebuildStore();
            Assert.AreEqual(5, store.GetNextSenderMsgSeqNum());
        }

        [Test]
        public void incNextSenderMsgSeqNumTest()
        {
            store.IncrNextSenderMsgSeqNum();
            Assert.AreEqual(2, store.GetNextSenderMsgSeqNum());
            rebuildStore();
            Assert.AreEqual(2, store.GetNextSenderMsgSeqNum());
        }

        [Test]
        public void getNextTargetMsgSeqNumTest()
        {
            Assert.AreEqual(1, store.GetNextTargetMsgSeqNum());
            store.SetNextTargetMsgSeqNum(6);
            Assert.AreEqual(6, store.GetNextTargetMsgSeqNum());
            rebuildStore();
            Assert.AreEqual(6, store.GetNextTargetMsgSeqNum());
        }

        [Test]
        public void incNextTargetMsgSeqNumTest()
        {
            store.IncrNextTargetMsgSeqNum();
            Assert.AreEqual(2, store.GetNextTargetMsgSeqNum());
            rebuildStore();
            Assert.AreEqual(2, store.GetNextTargetMsgSeqNum());
        }

        [Test]
        public void resetTest()
        {
            store.SetNextTargetMsgSeqNum(5);
            store.SetNextSenderMsgSeqNum(4);
            store.Reset();

            Assert.AreEqual(1, store.GetNextTargetMsgSeqNum());
            Assert.AreEqual(1, store.GetNextSenderMsgSeqNum());

            rebuildStore();
            Assert.AreEqual(1, store.GetNextTargetMsgSeqNum());
            Assert.AreEqual(1, store.GetNextSenderMsgSeqNum());

            store.SetNextTargetMsgSeqNum(5);
            store.SetNextSenderMsgSeqNum(6);

            Assert.AreEqual(5, store.GetNextTargetMsgSeqNum());
            Assert.AreEqual(6, store.GetNextSenderMsgSeqNum());

            rebuildStore();

            Assert.AreEqual(5, store.GetNextTargetMsgSeqNum());
            Assert.AreEqual(6, store.GetNextSenderMsgSeqNum());

            store.Set(1, "dude");
            store.Set(2, "pude");
            store.Set(3, "ok");
            store.Set(4, "ohai");

            store.Reset();

            var msgs = new List<string>();
            store.Get(2, 3, msgs);
            Assert.That(msgs,Is.Empty);

            rebuildStore();
            store.Get(2, 3, msgs);
            Assert.That(msgs, Is.Empty);

            store.Set(1, "dude");
            store.Set(2, "pude");
            store.Set(3, "ok");
            store.Set(4, "ohai");

            store.Get(2, 3, msgs);
            var expected = new List<string>() { "pude", "ok" };

            Assert.AreEqual(expected, msgs);

            rebuildStore();

            msgs = new List<string>();
            store.Get(2, 3, msgs);
        }


        [Test]
        public void getTest()
        {
            store.Set(1, "dude");
            store.Set(2, "pude");
            store.Set(3, "ok");
            store.Set(4, "ohai");

            var msgs = new List<string>();
            store.Get(2, 3, msgs);
            var expected = new List<string>() { "pude", "ok" };

            Assert.AreEqual(expected, msgs);

            rebuildStore();

            msgs = new List<string>();
            store.Get(2, 3, msgs);

            Assert.AreEqual(expected, msgs);
        }
        
    }
}