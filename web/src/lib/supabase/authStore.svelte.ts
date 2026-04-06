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
	let lastEvent = $state<AuthChangeEvent | null>(null);

	function setSession(nextSession: Session | null) {
		session = nextSession;
		user = nextSession?.user ?? null;
		initialized = true;
		if (!nextSession) {
			error = '';
		}
	}

	function setNotice(message: string) {
		notice = message;
	}

	function setError(message: string) {
		error = message;
	}

	function setRecoveryMode(nextRecoveryMode: boolean) {
		recoveryMode = nextRecoveryMode;
	}

	function handleAuthEvent(event: AuthChangeEvent, nextSession: Session | null) {
		lastEvent = event;
		setSession(nextSession);

		if (event === 'PASSWORD_RECOVERY') {
			recoveryMode = true;
			notice = 'Reset your password below.';
		}

		if (event === 'SIGNED_OUT') {
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

		recoveryMode = false;
		setSession(data.session ?? null);
		notice = data.session
			? 'Account created and signed in.'
			: 'Account created. Check your email to confirm sign-in.';
		return true;
	}

	async function requestPasswordReset(email: string, redirectTo: string): Promise<boolean> {
		loading = true;
		clearMessages();

		const supabase = getSupabaseBrowserClient();
		const { error: resetError } = await supabase.auth.resetPasswordForEmail(email, {
			redirectTo
		});

		loading = false;

		if (resetError) {
			error = resetError.message;
			return false;
		}

		notice = 'Password reset email sent. Check your inbox.';
		return true;
	}

	async function updatePassword(password: string): Promise<boolean> {
		loading = true;
		clearMessages();

		const supabase = getSupabaseBrowserClient();
		const { data, error: updateError } = await supabase.auth.updateUser({ password });

		loading = false;

		if (updateError) {
			error = updateError.message;
			return false;
		}

		recoveryMode = false;
		setSession(data.user ? session : null);
		notice = 'Password updated.';
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
		get lastEvent() { return lastEvent; },
		get isAuthenticated() { return user !== null; },
		setSession,
		handleAuthEvent,
		setNotice,
		setError,
		setRecoveryMode,
		clearMessages,
		signIn,
		signUp,
		requestPasswordReset,
		updatePassword,
		signOut
	};
}