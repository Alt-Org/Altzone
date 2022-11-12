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
            Debug.Log("test");
        }

        [TearDown]
        public void TearDown()
        {
            Debug.Log("test");
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
            var messageCounter = 0;

            this.Subscribe<Message>(MessageHandler);
            Assert.AreEqual(0, messageCounter);

            this.Publish(new Message(10));
            Assert.AreEqual(1, messageCounter);

            this.Unsubscribe<Message>(MessageHandler);
            Assert.AreEqual(1, messageCounter);

            this.Publish(new Message(20));

            Assert.AreEqual(1, messageCounter);

            this.Unsubscribe<Message>(MessageHandler);
            Assert.AreEqual(1, messageCounter);

            void MessageHandler(Message m)
            {
                messageCounter += 1;
                Debug.Log($"{m} : {messageCounter}");
            }
        }
    }
}