export function apiFetch(
  path: string, 
  init: RequestInit = {}
): Promise<Response> {
  const token = localStorage.getItem("token")
  // call fetch, passing same params, but add token to headers if it's not null
  return fetch(path, {
    ...init,
    headers: {
      ...(init.headers as Record<string, string>),
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
  })
}
