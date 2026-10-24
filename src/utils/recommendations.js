// Providers can return JSON, fenced JSON, or a plain numbered list.
export function normalizeRecommendations(value) {
  if (typeof value === 'string') {
    const text = value.trim().replace(/^```(?:json)?\s*/i, '').replace(/\s*```$/, '').trim();
    if (!text) return [];
    try {
      const parsed = JSON.parse(text);
      if (typeof parsed !== 'string' || parsed !== value) return normalizeRecommendations(parsed);
    } catch {
      // A plain-text response does not need JSON parsing.
    }
    // Do not display malformed structured output as a recommendation.
    if (/^[{[]/.test(text)) return [];
    return text.split(/\n\s*(?:\d+[.)]\s+|[-*•]\s+)?/)
      .map(line => line.replace(/^\s*(?:\d+[.)]|[-*•])\s+/, '').replace(/\*\*(.*?)\*\*/g, '$1').trim())
      .filter(Boolean);
  }
  if (Array.isArray(value)) {
    return [...new Set(value.flatMap(item => {
      if (item && typeof item === 'object' && !Array.isArray(item)) {
        const title = typeof item.title === 'string' ? item.title.trim() : '';
        const detail = item.description || item.tip || item.text || item.recommendation;
        if (typeof detail === 'string') return [title ? `${title}: ${detail.trim()}` : detail.trim()];
        return title ? [title] : [];
      }
      return normalizeRecommendations(item);
    }).filter(Boolean))];
  }
  if (value && typeof value === 'object') {
    return normalizeRecommendations(value.tips ?? value.recommendations ?? value.suggestions ?? []);
  }
  return [];
}
