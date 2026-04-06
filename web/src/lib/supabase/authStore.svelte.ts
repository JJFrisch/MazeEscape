import type { AuthChangeEvent, Session, User } from '@supabase/supabase-js';
import { getSupabaseBrowserClient } from './client';

export const authStore = createAuthStore();

function createAuthStore() {
	let session = $state<Session | null>(null);
	let user = $state<User | null>(null);
	let initialized = $state(false);
	let loading = $state(false);
	let error = $state('');
	let notice = $state('');
	let recoveryMode = $state(false);

	function setSession(nextSession: Session | null) {
		session = nextSession;
		user = nextSession?.user ?? null;
		initialized = true;
		if (!nextSession) {
			error = '';
			recoveryMode = false;
		}
	}

	function handleAuthEvent(event: AuthChangeEvent, nextSession: Session | null) {
		setSession(nextSession);
		if (event === 'PASSWORD_RECOVERY') {
			recoveryMode = true;
			notice = 'Reset your password below.';
		} else if (event === 'SIGNED_OUT') {
			recoveryMode = false;
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

		recoveryMode = false;
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
			: 'Account created. Check your email to confirm your account, then sign in.';
		return true;
	}

	async function requestPasswordReset(email: string): Promise<boolean> {
		loading = true;
		clearMessages();

		const supabase = getSupabaseBrowserClient();
		const redirectTo = typeof window !== 'undefined'
			? `${window.location.origin}${window.location.pathname}`
			: undefined;
		const { error: resetError } = await supabase.auth.resetPasswordForEmail(email.trim(), { redirectTo });

		loading = false;

		if (resetError) {
			error = resetError.message;
			return false;
		}

		notice = 'Password reset email sent. Check your inbox.';
		return true;
	}

	async function updatePassword(newPassword: string): Promise<boolean> {
		loading = true;
		clearMessages();

		const supabase = getSupabaseBrowserClient();
		const { error: updateError } = await supabase.auth.updateUser({ password: newPassword });

		loading = false;

		if (updateError) {
			error = updateError.message;
			return false;
		}

		recoveryMode = false;
		notice = 'Password updated. You are now signed in.';
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

		recoveryMode = false;
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
		get recoveryMode() { return recoveryMode; },
		get isAuthenticated() { return user !== null; },
		setSession,
		handleAuthEvent,
		clearMessages,
		signIn,
		signUp,
		requestPasswordReset,
		updatePassword,
		signOut
	};
}
