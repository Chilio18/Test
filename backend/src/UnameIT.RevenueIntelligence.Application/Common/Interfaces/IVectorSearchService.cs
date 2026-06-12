using UnameIT.RevenueIntelligence.Application.DTOs.Search;

namespace UnameIT.RevenueIntelligence.Application.Common.Interfaces;

public interface IVectorSearchService
{
    Task IndexDocumentAsync(string documentId, float[] embedding, string content,
        IDictionary<string, string> metadata, CancellationToken ct = default);

    Task<IReadOnlyList<SearchResultDto>> SearchAsync(float[] queryEmbedding, int topK,
        IDictionary<string, string>? filters = null, CancellationToken ct = default);

    Task DeleteDocumentAsync(string documentId, CancellationToken ct = default);
}
