namespace Hjg.Pngcs.Chunks
{
	public class ChunkPredicateId : ChunkPredicate
	{
		private readonly string id;

		public ChunkPredicateId(string id)
		{
			this.id = id;
		}

		public bool Matches(PngChunk c)
		{
			return c.Id.Equals(id);
		}
	}
}