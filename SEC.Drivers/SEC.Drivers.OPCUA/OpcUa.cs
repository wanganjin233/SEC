using Opc.Ua.Client;
using Opc.Ua;
using OpcUaHelper;
using SEC.Driver;
using SEC.Util;

namespace SEC.Drivers
{
    public class OpcUa : BaseDriver
    {
        private OpcUaClient opcUaClient = new OpcUaClient();
        /// <summary>
        /// opc.tcp://172.27.35.2:49320
        /// </summary>
        /// <param name="communicationStr"></param>
        /// <exception cref="Exception"></exception>
        public OpcUa(string communicationStr) : base(string.Empty)
        {
            if (communicationStr.Contains(';'))
            {
                string[] communicationStrs = communicationStr.Split(';');
                string[] user = communicationStrs[1].Split(':');
                opcUaClient.UserIdentity = new UserIdentity(user[0], user[1]);
                communicationStr = communicationStrs[0];
            }
            opcUaClient.ConnectServer(communicationStr).Wait();
        }
        public override void Start(int cycle = 100)
        {
            opcUaClient.AddSubscription("2258", "i=2258", CurrentTimeCallback);
            foreach (var tag in AllTagDic.Values)
            {
                opcUaClient.AddSubscription(tag.TagName, tag.Address, SubCallback);
            }
        }
        private DateTime CurrentTime { get; set; } = DateTime.MinValue;
        private void CurrentTimeCallback(string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args)
        {
            MonitoredItemNotification? notification = args.NotificationValue as MonitoredItemNotification;
            if ((DateTime)notification?.Value.WrappedValue.Value - CurrentTime >= TimeSpan.FromSeconds(1))
            {
                foreach (var tag in AllTagDic.Values)
                {
                    tag.Timestamp = DateTime.Now;
                }
            }

        }
        public override bool Write(Tag tag, object? value)
        {
            if (value == null)
            {
                return false;
            }
            return opcUaClient.WriteNode(tag.Address, tag.ObjectMatching(value));
        }
        private void SubCallback(string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs args)
        {
            MonitoredItemNotification? notification = args.NotificationValue as MonitoredItemNotification;
            AllTagDic[key].SetValue = notification?.Value.WrappedValue.Value;
        }
        protected override List<TagGroup>? Packet(List<Tag> tags)
        {
            TagGroups.Clear();
            TagGroup tagGroup = new TagGroup();
            tagGroup.Tags.AddRange(tags);
            TagGroups.Add(tagGroup);
            return TagGroups.ToList();
        }
    }
}