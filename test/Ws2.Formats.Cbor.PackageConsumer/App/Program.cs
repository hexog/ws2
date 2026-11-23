using System.Formats.Cbor;
using PackageConsumer.Models;

var original = new Message(7, null, new[] { 2, 3, 5 }, new Child(11), new Version(1, 2, 3));
var writer = new CborWriter();
original.Serialize(writer);

var reader = new CborReader(writer.Encode());
var copy = Message.Parse(reader);
if (reader.BytesRemaining != 0)
{
    throw new InvalidOperationException("The generated parser left trailing CBOR data.");
}

if (copy.Id != 7 || copy.Name is not null || !copy.Values.SequenceEqual(new[] { 2, 3, 5 }) ||
    copy.Child?.Value != 11 || copy.Version != new Version(1, 2, 3))
{
    throw new InvalidOperationException("The generated package-consumer round trip failed.");
}

var mutable = new MutableMessage
{
    Number = 19,
    Label = "mutable",
    Values = new[] { 4, 6, 9 },
    Version = new Version(2, 4)
};
var mutableWriter = new CborWriter();
mutable.Serialize(mutableWriter);
var mutableReader = new CborReader(mutableWriter.Encode());
var mutableCopy = MutableMessage.Parse(mutableReader);
if (mutableReader.BytesRemaining != 0 || mutableCopy.Number != 19 || mutableCopy.Label != "mutable" ||
    !mutableCopy.Values.SequenceEqual(new[] { 4, 6, 9 }) || mutableCopy.Version != new Version(2, 4))
{
    throw new InvalidOperationException("The parameterless package-consumer round trip failed.");
}

Console.WriteLine($"CBOR package consumer passed on {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}.");
