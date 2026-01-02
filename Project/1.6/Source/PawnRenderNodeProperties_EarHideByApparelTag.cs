using System.Collections.Generic;

namespace Verse
{
	public class PawnRenderNodeProperties_EarHideByApparelTag : PawnRenderNodeProperties
	{
		public List<string> hiddenUnderApparelTags;

		public PawnRenderNodeProperties_EarHideByApparelTag()
		{
			this.workerClass = typeof(PawnRenderNodeWorker_EarHideByApparelTag);
		}
	}
}

