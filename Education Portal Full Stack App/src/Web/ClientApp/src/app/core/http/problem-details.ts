/**
 * Reads the messages from an API error. A 400 carries RFC 9457 validation problem details with an `errors` object per
 * field (FluentValidation on the server, already in the page's language); anything else gets one plain message. Server
 * text is shown as text only.
 */
export function problemMessages(error: unknown): string[] {
  const status = typeof error === 'object' && error !== null && 'status' in error ? Number(error.status) : undefined;
  const body = typeof error === 'object' && error !== null && 'response' in error ? error.response : undefined;

  if (status === 400 && typeof body === 'string') {
    try {
      const problem = JSON.parse(body) as { errors?: Record<string, unknown> };
      const messages = Object.values(problem.errors ?? {})
        .flatMap((value) => (Array.isArray(value) ? value : []))
        .filter((value): value is string => typeof value === 'string');
      if (messages.length > 0) {
        return messages;
      }
    } catch {
      // Not JSON: fall through to the generic message below.
    }
    return [$localize`:@@problem.invalid:The request is not valid. Check the fields and try again.`];
  }

  switch (status) {
    case 401:
      return [$localize`:@@problem.sessionEnded:Your session has ended. Sign in again.`];
    case 403:
      return [$localize`:@@problem.forbidden:You do not have permission to do this.`];
    case 404:
      return [$localize`:@@problem.notFound:It was not found. It may have been removed.`];
    default:
      return [$localize`:@@problem.unexpected:Something went wrong. Try again in a moment.`];
  }
}
