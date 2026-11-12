import { describe, expect, it } from 'vitest';
import { problemMessages } from './problem-details';

describe('problemMessages', () => {
  it('lists the field errors of a validation problem', () => {
    const error = { status: 400, response: JSON.stringify({ errors: { Logo: ['The logo is too large.'], SidebarColor: ['Bad colour.'] } }) };

    expect(problemMessages(error)).toEqual(['The logo is too large.', 'Bad colour.']);
  });

  it('gives a plain message for other failures', () => {
    expect(problemMessages({ status: 403, response: '' })).toEqual(['You do not have permission to do this.']);
    expect(problemMessages(new Error('network'))).toEqual(['Something went wrong. Try again in a moment.']);
  });
});
