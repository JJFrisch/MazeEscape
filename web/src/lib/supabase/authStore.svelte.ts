import type { Session, User } from '@supabase/supabase-js';
import { getSupabaseBrowserClient } from './client';

export const authStore = createAuthStore();

function createAuthStore() {
	let session = $state<Session | null>(null);
	let user = $state<User | null>(null);
	let initialized = $state(false);
	let loading = $state(false);
	let error = $state('');
	let notice = $state('');

	function setSession(nextSession: Session | null) {
		session = nextSession;
		user = nextSession?.user ?? null;
		initialized = true;
		if (!nextSession) {
			error = '';
		}
	}

	function clearMessages() {
		error = '';
		notice = '';
	}

	async function signIn(email: string, password: string): Promise<boolean> {
		loading = true;
		clearMessages();

		const supabase = getSupabaseBrowserClient();
		const { data, error: signInError } = await supabase.auth.signInWithPassword({ email, password });

		loading = false;

		if (signInError) {
			error = signInError.message;
			return false;
		}

		setSession(data.session ?? null);
		notice = 'Signed in.';
		return true;
	}

	async function signUp(email: string, password: string): Promise<boolean> {
		loading = true;
		clearMessages();

		const supabase = getSupabaseBrowserClient();
		const { data, error: signUpError } = await supabase.auth.signUp({ email, password });

		loading = false;

		if (signUpError) {
			error = signUpError.message;
			return false;
		}

		setSession(data.session ?? null);
		notice = data.session
			? 'Account created and signed in.'
			: 'Account created. Check your email to confirm sign-in.';
		return true;
	}

	async function signOut(): Promise<boolean> {
		loading = true;
		clearMessages();

		const supabase = getSupabaseBrowserClient();
		const { error: signOutError } = await supabase.auth.signOut();

		loading = false;

		if (signOutError) {
			error = signOutError.message;
			return false;
		}

		setSession(null);
		notice = 'Signed out.';
		return true;
	}

	return {
		get session() { return session; },
		get user() { return user; },
		get initialized() { return initialized; },
		get loading() { return loading; },
		get error() { return error; },
		get notice() { return notice; },
		get isAuthenticated() { return user !== null; },
		setSession,
		clearMessages,
		signIn,
		signUp,
		signOut
	};
}