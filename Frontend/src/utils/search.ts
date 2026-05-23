export function normalizeSearch(value: unknown) {
  return String(value ?? '').toLowerCase().trim();
}

export function matchesSearch(query: string, values: unknown[]) {
  const q = normalizeSearch(query);
  if (!q) return true;
  return values.some((value) => normalizeSearch(value).includes(q));
}
