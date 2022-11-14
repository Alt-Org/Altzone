using NUnit.Framework;
using Prg.Scripts.Common.PubSub;
using Assert = UnityEngine.Assertions.Assert;

namespace Tests.EditMode.PubSub
{
    [TestFixture]
    public class PubSubTest
    {
        private class Message
        {
            public readonly int Id;

            public Message(int id)
            {
                Id = id;
            }

            public override string ToString()
            {
                return $"{nameof(Id)}: {Id}";
            }
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Debug.Log("setup");
        }

        [TearDown]
        public void TearDown()
        {
            Debug.Log("reset");
            this.Unsubscribe();
        }

        [Test]
        public void PublishOneMessage()
        {
            Debug.Log("test");
            var message = new Message(1);
            this.Subscribe<Message>((m) =>
            {
                Debug.Log($"{m}");
                Assert.AreEqual(message.Id, m.Id);
            });
            this.Publish(message);
        }

        [Test]
        public void PublishTwoMessages()
        {
            Debug.Log("test");
            this.Subscribe<Message>((m) =>
            {
                Debug.Log($"{m}");
                Assert.AreEqual(0, m.Id % 33);
            });
            this.Publish(new Message(33));
            this.Publish(new Message(66));
        }

        [Test]
        public void UnsubscribeTwice()
        {
            Debug.Log("test");
            var messageCounter1 = 0;
            var messageCounter2 = 0;

            this.Subscribe<Message>(MessageHandler1);
            this.Subscribe<Message>(MessageHandler2);
            Assert.AreEqual(0, messageCounter1);
            Assert.AreEqual(0, messageCounter2);

            this.Publish(new Message(10));
            Assert.AreEqual(1, messageCounter1);
            Assert.AreEqual(1, messageCounter2);

            this.Unsubscribe<Message>(MessageHandler1);
            Assert.AreEqual(1, messageCounter1);
            Assert.AreEqual(1, messageCounter2);

            this.Publish(new Message(20));
            Assert.AreEqual(1, messageCounter1);
            Assert.AreEqual(2, messageCounter2);

            // Should fail with exception!
            this.Unsubscribe<Message>(MessageHandler1);

            void MessageHandler1(Message m)
            {
                messageCounter1 += 1;
                Debug.Log($"h1 {m} : {messageCounter1}");
            }

            void MessageHandler2(Message m)
            {
                messageCounter2 += 1;
                Debug.Log($"h2 {m} : {messageCounter2}");
            }
        }

        [Test]
        public void PredicateTest()
        {
            Debug.Log("test");
            var message1 = new Message(1);
            var message2 = new Message(2);
            var message3 = new Message(3);
            var messageCounter = 0;

            this.Subscribe<Message>(MessageHandler);
            this.Subscribe<Message>(MessageHandler1, message => message.Id == 1);
            this.Subscribe<Message>(MessageHandler2, message => message.Id == 2);

            this.Publish(message1);
            this.Publish(message2);
            this.Publish(message3);
            Assert.AreEqual(3 + 2, messageCounter);

            void MessageHandler(Message m)
            {
                messageCounter += 1;
                Debug.Log($"h0 {m} : {messageCounter}");
            }

            void MessageHandler1(Message m)
            {
                messageCounter += 1;
                Debug.Log($"h1 {m} : {messageCounter}");
                Assert.AreEqual(1, m.Id);
            }

            void MessageHandler2(Message m)
            {
                messageCounter += 1;
                Debug.Log($"h2 {m} : {messageCounter}");
                Assert.AreEqual(2, m.Id);
            }
        }
    }
}